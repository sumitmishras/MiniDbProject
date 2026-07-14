using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;

namespace MiniDbWpf.Services;

public class DatabaseService : IDatabaseService
{
    private string BuildConnStr(string server, string database, string? user, string? password)
    {
        var b = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            TrustServerCertificate = true,
            ConnectTimeout = 30
        };
        if (!string.IsNullOrEmpty(user))
        { b.UserID = user; b.Password = password; }
        else
        { b.IntegratedSecurity = true; }
        return b.ConnectionString;
    }

    public async Task<(bool Success, string Message)> ValidateConnectionAsync(
        string server, string database, string? user, string? password)
    {
        try
        {
            using var conn = new SqlConnection(BuildConnStr(server, database, user, password));
            await conn.OpenAsync();
            return (true, "Connection successful.");
        }
        catch (Exception ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
    }

    public async Task CreateDatabaseAsync(
        string sourceServer, string sourceDatabase, string? sourceUser, string? sourcePassword,
        string destinationServer, string destinationDatabase,
        DateTime fromDate, DateTime toDate, bool debug,
        IProgress<(int Percent, string Stage, string Task, string Detail)> progress,
        CancellationToken ct)
    {
        var destConnStr = BuildConnStr(destinationServer, "master", null, null);
        var tempDir = @"C:\Temp_MiniDB";
        var dacpacPath = Path.Combine(tempDir, "MiniDB.dacpac");
        Directory.CreateDirectory(tempDir);

        // Step 1: Create destination database
        progress.Report((5, "Setup", "Creating destination database...", $"CREATE DATABASE [{destinationDatabase}]"));
        ct.ThrowIfCancellationRequested();
        using (var conn = new SqlConnection(destConnStr))
        {
            await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = @db)
                BEGIN
                    CREATE DATABASE [{destinationDatabase}];
                    PRINT 'Database created.';
                END
                ELSE
                    PRINT 'Database already exists.';";
            cmd.Parameters.AddWithValue("@db", destinationDatabase);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        progress.Report((15, "Setup", "Destination database ready.", ""));

        // Step 2: Extract schema from source via sqlpackage
        progress.Report((20, "Extract", "Extracting schema from source...", dacpacPath));
        ct.ThrowIfCancellationRequested();

        var extractArgs = new StringBuilder();
        extractArgs.Append("/Action:Extract ");
        extractArgs.Append($@"/SourceServerName:""{sourceServer}"" ");
        extractArgs.Append($@"/SourceDatabaseName:""{sourceDatabase}"" ");
        if (!string.IsNullOrEmpty(sourceUser))
        {
            extractArgs.Append($@"/SourceUser:""{sourceUser}"" ");
            extractArgs.Append($@"/SourcePassword:""{sourcePassword}"" ");
        }
        else
        {
            extractArgs.Append("/SourceIntegratedSecurity:true ");
        }
        extractArgs.Append("/SourceTrustServerCertificate:true ");
        extractArgs.Append($@"/TargetFile:""{dacpacPath}"" ");
        extractArgs.Append("/p:ExtractAllTableData=false");

        var extractResult = await RunSqlPackageAsync(extractArgs.ToString(), ct,
            msg => progress.Report((35, "Extract", "Extracting...", msg)));

        progress.Report((45, "Extract", "Schema extracted successfully.",
            extractResult.Take(200).ToString() ?? ""));

        // Step 3: Publish schema to destination via sqlpackage
        progress.Report((50, "Publish", "Publishing schema to destination...", ""));
        ct.ThrowIfCancellationRequested();

        var publishArgs = new StringBuilder();
        publishArgs.Append("/Action:Publish ");
        publishArgs.Append($@"/SourceFile:""{dacpacPath}"" ");
        publishArgs.Append($@"/TargetServerName:""{destinationServer}"" ");
        publishArgs.Append($@"/TargetDatabaseName:""{destinationDatabase}"" ");
        publishArgs.Append("/TargetTrustServerCertificate:true ");
        publishArgs.Append("/p:BlockOnPossibleDataLoss=false");

        var publishResult = await RunSqlPackageAsync(publishArgs.ToString(), ct,
            msg => progress.Report((65, "Publish", "Publishing...", msg)));

        progress.Report((75, "Publish", "Schema published to destination.", ""));
        var destDbConnStr = BuildConnStr(destinationServer, destinationDatabase, null, null);

        // Step 4: Ensure MiniDB procedures exist on destination
        await EnsureMiniDbProceduresAsync(destDbConnStr, progress);

        // Step 5: Run createMiniDB procedure
        progress.Report((78, "Data Load", "Running createMiniDB procedure...",
            $"EXEC dbo.createMiniDB @SourceServer='{sourceServer}', @SourceDB='{sourceDatabase}', @FromDate='{fromDate:yyyy-MM-dd}', @ToDate='{toDate:yyyy-MM-dd}', @Debug={(debug ? 0 : 1)}"));
        ct.ThrowIfCancellationRequested();

        using (var conn = new SqlConnection(destDbConnStr))
        {
            await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.createMiniDB";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SourceDB", sourceDatabase);
            cmd.Parameters.AddWithValue("@FromDate", fromDate);
            cmd.Parameters.AddWithValue("@ToDate", toDate);
            cmd.Parameters.AddWithValue("@Debug", debug ? 0 : 1);
            cmd.Parameters.AddWithValue("@SourceServer", sourceServer);

            var output = new StringBuilder();
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                output.AppendLine(reader[0]?.ToString());

            progress.Report((90, "Data Load", "Data load complete.", output.ToString()));
        }

        // Step 6: Run checkMiniDBData
        progress.Report((95, "Validation", "Running checkMiniDBData...", ""));
        ct.ThrowIfCancellationRequested();

        var checkResult = await RunCheckDataAsync(destinationServer, destinationDatabase, null, null);
        progress.Report((100, "Complete", "MiniDB created successfully!", checkResult));
    }

    public async Task<string> RunCheckDataAsync(string server, string database, string? user, string? password)
    {
        var result = new StringBuilder();
        try
        {
            using var conn = new SqlConnection(BuildConnStr(server, database, user, password));
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.checkMiniDBData";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var values = new object[reader.FieldCount];
                reader.GetValues(values);
                result.AppendLine(string.Join(" | ", values));
            }
        }
        catch (Exception ex)
        {
            result.AppendLine($"checkMiniDBData error: {ex.Message}");
        }
        return result.ToString();
    }

    private async Task EnsureMiniDbProceduresAsync(string connStr,
        IProgress<(int Percent, string Stage, string Task, string Detail)> progress)
    {
        progress.Report((76, "Setup", "Ensuring MiniDB procedures on destination...", ""));
        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = GetMiniDbProcedureSql();
        await cmd.ExecuteNonQueryAsync();
    }

    private static string GetMiniDbProcedureSql()
    {
        return @"
CREATE OR ALTER PROCEDURE dbo.createMiniDB
(@SourceDB SYSNAME = NULL, @FromDate DATE = NULL, @ToDate DATE = NULL, @Debug BIT = 0, @SourceServer SYSNAME = NULL)
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 DECLARE @CurrentDB SYSNAME = DB_NAME();
 IF @SourceDB IS NULL OR @SourceDB = N'' BEGIN RAISERROR('@SourceDB is required.',16,1); RETURN; END
 IF @FromDate IS NULL BEGIN RAISERROR('@FromDate is required.',16,1); RETURN; END
 IF @ToDate IS NULL BEGIN RAISERROR('@ToDate is required.',16,1); RETURN; END
 IF @ToDate < @FromDate BEGIN RAISERROR('@ToDate must be >= @FromDate.',16,1); RETURN; END
 IF @SourceServer IS NOT NULL AND @SourceServer <> N''
 BEGIN
  DECLARE @ThisServer SYSNAME = CAST(SERVERPROPERTY(''ServerName'') AS SYSNAME);
  DECLARE @ThisMachine SYSNAME = CAST(SERVERPROPERTY(''MachineName'') AS SYSNAME);
  DECLARE @ThisNetName SYSNAME = CAST(ISNULL(SERVERPROPERTY(''ComputerNamePhysicalNetBIOS''),N'') AS SYSNAME);
  IF @SourceServer = @ThisServer OR @SourceServer = @ThisMachine OR @SourceServer = @ThisNetName
     OR @SourceServer = @@SERVERNAME OR LOWER(@SourceServer) IN (N''(local)'',N''localhost'',N''.'')
   SET @SourceServer = NULL;
 END
 IF (@SourceServer IS NULL OR @SourceServer = N'')
 BEGIN
  IF DB_ID(@SourceDB) IS NULL BEGIN RAISERROR(''Source db [%s] not found on this server.'',16,1,@SourceDB); RETURN; END
  IF QUOTENAME(@SourceDB) = QUOTENAME(@CurrentDB) BEGIN RAISERROR(''Source and destination must differ.'',16,1); RETURN; END
 END
 ELSE
 BEGIN
  IF NOT EXISTS (SELECT 1 FROM sys.servers WHERE name = @SourceServer AND is_linked = 1)
  BEGIN
   IF DB_ID(@SourceDB) IS NOT NULL SET @SourceServer = NULL;
   ELSE BEGIN RAISERROR(''Source [%s] not a linked server or local db.'',16,1,@SourceServer); RETURN; END
  END
 END
 DECLARE @SrcPrefix NVARCHAR(300) = CASE WHEN @SourceServer IS NULL THEN QUOTENAME(@SourceDB)+N''.'' ELSE QUOTENAME(@SourceServer)+N''.''+QUOTENAME(@SourceDB)+N''.'' END;
 DECLARE @Tables TABLE (ProcessOrder INT IDENTITY(1,1), SchemaName SYSNAME, TableName SYSNAME, FilterType VARCHAR(20), FilterCol SYSNAME NULL, CustomWhere NVARCHAR(MAX) NULL, LiteralName NVARCHAR(300) NULL);
 INSERT INTO @Tables(SchemaName,TableName,FilterType,FilterCol) VALUES
 (''common'',''encounters'' ,''ENCOUNTERS_DATE'',NULL)
,(''common'',''physician_encounter'' ,''ENCOUNTER_ID'' ,''encounter_id'')
,(''rcm'',''cob_order'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''rcm'',''patient_insurances'' ,''PID'' ,''pid'')
,(''common'',''accessioning_Status'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''rcm'',''chargemaster'' ,''NONE'' ,NULL)
,(''rcm'',''visit_charges'' ,''ENCOUNTER_ID'' ,''encounter_id'')
,(''rcm'',''visit_codes'' ,''ENCOUNTER_ID'' ,''encounter_id'')
,(''common'',''documents'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''rcm'',''transactions'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''common'',''facilities'' ,''NONE'' ,NULL)
,(''rcm'',''claim_batches'' ,''CUSTOM'' ,NULL)
,(''rcm'',''claims'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''common'',''casetypes'' ,''NONE'' ,NULL)
,(''common'',''carreport'' ,''SAMPLE_ID'' ,''specimen_id'')
,(''common'',''transactionreport'' ,''SAMPLE_ID'' ,''specimen_id'')
,(''common'',''insurance'' ,''NONE'' ,NULL)
,(''common'',''reference_data'' ,''NONE'' ,NULL)
,(''common'',''users'' ,''NONE'' ,NULL)
,(''common'',''casetypeview'' ,''NONE'' ,NULL)
,(''common'',''account_groups'' ,''NONE'' ,NULL)
,(''common'',''account_managers'' ,''NONE'' ,NULL)
,(''common'',''account_managers_practice'',''CUSTOM'' ,NULL)
,(''common'',''additionl_discount_factor_forcast'',''NONE'',NULL)
,(''common'',''AmountMissingQuery'' ,''CHECK_NO'' ,''checkno'')
,(''common'',''Ar_bucket_carc_code'' ,''NONE'' ,NULL)
,(''rcm'',''ar_posting'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''common'',''ar_worksheets'' ,''SAMPLE_ID'' ,''claimid'')
,(''rcm'',''assign_eob_to_batch'' ,''CLAIM_BATCH'' ,''batch_id'')
,(''rcm'',''assign_remits'' ,''CUSTOM'' ,NULL)
,(''common'',''carreport_AR'' ,''SAMPLE_ID'' ,''specimen_id'')
,(''common'',''casetypes_discount_factor'',''NONE'',NULL)
,(''rcm'',''check_reset_history'' ,''CUSTOM'' ,NULL)
,(''common'',''Claim835Header'' ,''CUSTOM'' ,NULL)
,(''common'',''Claim835Details'' ,''EOB_ID'' ,''eobid'')
,(''common'',''Claim835ServiceDetails'' ,''CUSTOM'' ,NULL)
,(''common'',''Claim835CASDetails'' ,''CUSTOM'' ,NULL)
,(''common'',''Claim835AdditionalAdjustment'',''CUSTOM'',NULL)
,(''common'',''claim_reason_master'' ,''ENCOUNTER_ID'' ,''encounter_id'')
,(''common'',''Claim835PLB'' ,''EOB_ID'' ,''eobid'')
,(''common'',''Claim835SupplementalPayment'',''CUSTOM'',NULL)
,(''common'',''cog_settings'' ,''NONE'' ,NULL)
,(''common'',''insurance_verification'' ,''PID'' ,''pid'')
,(''common'',''namematching'' ,''NONE'' ,NULL)
,(''common'',''namematchingpending'' ,''NONE'' ,NULL)
,(''rcm'',''patient_statements'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''common'',''patients'' ,''PID'' ,''pid'')
,(''common'',''patients_notes'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''rcm'',''payment_batches'' ,''EOB_ID'' ,''EOBID'')
,(''rcm'',''payment_plans'' ,''ENCOUNTER_ID'' ,''encounter_id'')
,(''rcm'',''payments'' ,''CHECK_NO'' ,''check_number'')
,(''rcm'',''pending_adjustment'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''common'',''physicians'' ,''CUSTOM'' ,NULL)
,(''common'',''post_payment_review'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''common'',''PostedEOBBatchMapping'' ,''EOB_ID'' ,''eobid'')
,(''common'',''ppr_document_notes'' ,''CUSTOM'' ,NULL)
,(''common'',''Reference_Lab'' ,''NONE'' ,NULL)
,(''common'',''documents_scan_analysis'' ,''ENCOUNTER_ID'' ,''encounter_id'')
,(''rcm'',''EDINotes'' ,''CLAIM_BATCH'' ,''claim_batches_id'')
,(''rcm'',''encounter_batch_detail'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''rcm'',''encounter_reasons'' ,''ENCOUNTER_ID'' ,''encounter_id'')
,(''rcm'',''eob_patient'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''rcm'',''eob_transaction'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''rcm'',''EobPostingFiles'' ,''NONE'' ,NULL)
,(''rcm'',''expected_reimbursement_config'',''NONE'',NULL)
,(''common'',''ftr_remarks_master'' ,''NONE'' ,NULL)
,(''common'',''hl7_inbound'' ,''SAMPLE_ID'' ,''sampleid'')
,(''common'',''hl7_tag_master'' ,''NONE'' ,NULL)
,(''common'',''DEFICIENCY_DENIAL_TABLE'' ,''SAMPLE_ID'' ,''sample_id'')
,(''common'',''denial_carc_codes'' ,''NONE'' ,NULL)
,(''rcm'',''inserteob_from_service'' ,''EOB_ID'' ,''eob_id'')
,(''common'',''Inbound_data'' ,''LEGACY_ID'' ,''caseno'')
,(''common'',''Inbound_data_log'' ,''LEGACY_ID'' ,''caseno'')
,(''rcm'',''visit_hold_reasons'' ,''ENCOUNTER_ID'' ,''visit_id'')
,(''rcm'',''weekly_report'' ,''NONE'' ,NULL)
,(''rcm'',''waystar_eligibility_response'',''SAMPLE_ID'',''sample_id'');
 INSERT INTO @Tables(SchemaName,TableName,FilterType,FilterCol,CustomWhere,LiteralName) VALUES
 (''dbo'',''common.practice'',''CUSTOM'',NULL,N''practice_id in (select distinct practice_id from common.physician_encounter)'',N''[common.practice]'');
 DECLARE @Order INT,@Schema SYSNAME,@Table SYSNAME,@FilterType VARCHAR(20),@FilterCol SYSNAME,@CustomWhere NVARCHAR(MAX),@ColList NVARCHAR(MAX),@HasIdentity BIT,@WhereSql NVARCHAR(MAX),@Sql NVARCHAR(MAX),@TgtObjId INT,@SrcName NVARCHAR(400),@TgtName NVARCHAR(300),@LiteralName NVARCHAR(300),@ObjPart NVARCHAR(300),@SrcObjId INT;
 DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT ProcessOrder,SchemaName,TableName,FilterType,FilterCol,CustomWhere,LiteralName FROM @Tables ORDER BY ProcessOrder;
 OPEN c; FETCH NEXT FROM c INTO @Order,@Schema,@Table,@FilterType,@FilterCol,@CustomWhere,@LiteralName;
 WHILE @@FETCH_STATUS = 0
 BEGIN
  BEGIN TRY
   SET @ObjPart = CASE WHEN @LiteralName IS NOT NULL THEN @LiteralName ELSE QUOTENAME(@Table) END;
   SET @TgtName = QUOTENAME(@Schema)+N''.''+@ObjPart;
   SET @SrcName = @SrcPrefix+QUOTENAME(@Schema)+N''.''+@ObjPart;
   SET @TgtObjId = OBJECT_ID(@TgtName);
   IF @TgtObjId IS NULL BEGIN PRINT ''-- SKIP (not in current db): ''+@TgtName; GOTO NextRow; END
   IF (@SourceServer IS NULL OR @SourceServer = N'''')
   BEGIN
    SET @Sql = N''SELECT @oid = OBJECT_ID(@n)'';
    EXEC sp_executesql @Sql,N''@n NVARCHAR(400), @oid INT OUTPUT'',@n=@SrcName,@oid=@SrcObjId OUTPUT;
    IF @SrcObjId IS NULL BEGIN PRINT ''-- SKIP (not in source db): ''+@SrcName; GOTO NextRow; END
   END
   ELSE
   BEGIN
    DECLARE @SrcExists INT=0; DECLARE @SrcTblName SYSNAME = CASE WHEN @LiteralName IS NOT NULL THEN REPLACE(REPLACE(@LiteralName,''['',''''),'']'','''') ELSE @Table END;
    SET @Sql = N''SELECT @e = COUNT(*) FROM ''+QUOTENAME(@SourceServer)+N''.''+QUOTENAME(@SourceDB)+N''.INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @sch AND TABLE_NAME = @tbl;'';
    EXEC sp_executesql @Sql,N''@sch SYSNAME, @tbl SYSNAME, @e INT OUTPUT'',@sch=@Schema,@tbl=@SrcTblName,@e=@SrcExists OUTPUT;
    IF @SrcExists=0 BEGIN PRINT ''-- SKIP (not in source db): ''+@SrcName; GOTO NextRow; END
   END
   DECLARE @TgtCols TABLE(name SYSNAME,column_id INT); DELETE FROM @TgtCols;
   INSERT INTO @TgtCols(name,column_id) SELECT c.name,c.column_id FROM sys.columns c WHERE c.object_id=@TgtObjId AND c.is_computed=0 AND c.system_type_id<>189;
   DECLARE @SrcCols TABLE(name SYSNAME); DELETE FROM @SrcCols; DECLARE @ColSql NVARCHAR(MAX);
   IF (@SourceServer IS NULL OR @SourceServer = N'''')
   BEGIN
    SET @ColSql = N''SELECT c.name COLLATE DATABASE_DEFAULT FROM ''+QUOTENAME(@SourceDB)+N''.sys.columns c WHERE c.object_id=@sid AND c.is_computed=0 AND c.system_type_id<>189;'';
    INSERT INTO @SrcCols(name) EXEC sp_executesql @ColSql,N''@sid INT'',@sid=@SrcObjId;
   END
   ELSE
   BEGIN
    SET @SrcTblName = CASE WHEN @LiteralName IS NOT NULL THEN REPLACE(REPLACE(@LiteralName,''['',''''),'']'','''') ELSE @Table END;
    SET @ColSql = N''SELECT COLUMN_NAME COLLATE DATABASE_DEFAULT FROM ''+QUOTENAME(@SourceServer)+N''.''+QUOTENAME(@SourceDB)+N''.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=@sch AND TABLE_NAME=@tbl;'';
    INSERT INTO @SrcCols(name) EXEC sp_executesql @ColSql,N''@sch SYSNAME, @tbl SYSNAME'',@sch=@Schema,@tbl=@SrcTblName;
   END
   SELECT @ColList=STRING_AGG(QUOTENAME(t.name),N'','') WITHIN GROUP(ORDER BY t.column_id) FROM @TgtCols t WHERE EXISTS(SELECT 1 FROM @SrcCols s WHERE s.name=t.name COLLATE DATABASE_DEFAULT);
   IF @ColList IS NULL BEGIN PRINT ''-- SKIP (no common columns): ''+@TgtName; GOTO NextRow; END
   SELECT @HasIdentity = CASE WHEN EXISTS(SELECT 1 FROM sys.identity_columns WHERE object_id=@TgtObjId) THEN 1 ELSE 0 END;
   SET @WhereSql = CASE @FilterType
    WHEN ''ENCOUNTERS_DATE'' THEN N''WHERE dos >= @FromDate AND dos < DATEADD(DAY,1,@ToDate)''
    WHEN ''ENCOUNTER_ID'' THEN N''WHERE ''+QUOTENAME(@FilterCol)+N'' IN (SELECT encounter_id FROM common.encounters WITH(NOLOCK))''
    WHEN ''PID'' THEN N''WHERE ''+QUOTENAME(@FilterCol)+N'' IN (SELECT pid FROM common.encounters WITH(NOLOCK))''
    WHEN ''SAMPLE_ID'' THEN N''WHERE ''+QUOTENAME(@FilterCol)+N'' IN (SELECT sample_id FROM common.encounters WITH(NOLOCK))''
    WHEN ''LEGACY_ID'' THEN N''WHERE ''+QUOTENAME(@FilterCol)+N'' IN (SELECT legacy_id FROM common.encounters WITH(NOLOCK))''
    WHEN ''CHECK_NO'' THEN N''WHERE ''+QUOTENAME(@FilterCol)+N'' IN (SELECT check_no FROM rcm.transactions WITH(NOLOCK))''
    WHEN ''EOB_ID'' THEN N''WHERE ''+QUOTENAME(@FilterCol)+N'' IN (SELECT eobid FROM common.Claim835Header WITH(NOLOCK))''
    WHEN ''CLAIM_BATCH'' THEN N''WHERE ''+QUOTENAME(@FilterCol)+N'' IN (SELECT claim_batches_id FROM rcm.claim_batches WITH(NOLOCK))''
    WHEN ''CUSTOM'' THEN N''WHERE ''+REPLACE(@CustomWhere,N''{SRC}'',CASE WHEN @SourceServer IS NULL THEN QUOTENAME(@SourceDB) ELSE QUOTENAME(@SourceServer)+N''.''+QUOTENAME(@SourceDB) END)
    ELSE N'''' END;
   SET @Sql = CASE WHEN @HasIdentity=1 THEN N''SET IDENTITY_INSERT ''+@TgtName+N'' ON;''+CHAR(13) ELSE N'''' END
     + N''INSERT INTO ''+@TgtName+N'' (''+@ColList+N'')''+CHAR(13)
     + N''SELECT ''+@ColList+N'' FROM ''+@SrcName+N'' AS src WITH(NOLOCK) ''+CHAR(13)
     + @WhereSql+N'';''+CHAR(13)
     + CASE WHEN @HasIdentity=1 THEN N''SET IDENTITY_INSERT ''+@TgtName+N'' OFF;'' ELSE N'''' END;
   IF @Debug=0 PRINT @Sql; ELSE BEGIN EXEC sp_executesql @Sql,N''@FromDate DATE, @ToDate DATE'',@FromDate=@FromDate,@ToDate=@ToDate; PRINT ''OK [''+CAST(@Order AS VARCHAR(5))+N''] ''+@TgtName+'' rows=''+CAST(@@ROWCOUNT AS VARCHAR(20)); END
  END TRY
  BEGIN CATCH
   IF @HasIdentity=1 BEGIN TRY EXEC(N''SET IDENTITY_INSERT ''+@TgtName+N'' OFF;''); END TRY BEGIN CATCH END CATCH END TRY; PRINT ''ERROR on ''+@TgtName+'' -> ''+ERROR_MESSAGE();
  END CATCH
  NextRow: FETCH NEXT FROM c INTO @Order,@Schema,@Table,@FilterType,@FilterCol,@CustomWhere,@LiteralName;
 END
 CLOSE c; DEALLOCATE c;
 PRINT ''createMiniDB complete.'';
END;

CREATE OR ALTER PROCEDURE dbo.checkMiniDBData
AS
DECLARE @Driver TABLE (ord INT IDENTITY(1,1), SchemaName SYSNAME, TableName SYSNAME);
INSERT INTO @Driver (SchemaName, TableName) VALUES
 (''common'',''encounters''),(''common'',''physician_encounter''),(''rcm'',''cob_order'')
,(''rcm'',''patient_insurances''),(''common'',''accessioning_Status''),(''rcm'',''chargemaster'')
,(''rcm'',''visit_charges''),(''rcm'',''visit_codes''),(''common'',''documents'')
,(''rcm'',''transactions''),(''common'',''facilities''),(''rcm'',''claims'')
,(''common'',''casetypes''),(''common'',''carreport''),(''common'',''transactionreport'')
,(''common'',''insurance''),(''common'',''reference_data''),(''common'',''users'')
,(''common'',''casetypeview''),(''common'',''account_groups''),(''common'',''account_managers'')
,(''common'',''account_managers_practice''),(''common'',''additionl_discount_factor_forcast'')
,(''common'',''AmountMissingQuery''),(''common'',''Ar_bucket_carc_code''),(''rcm'',''ar_posting'')
,(''common'',''ar_worksheets''),(''rcm'',''claim_batches''),(''rcm'',''assign_eob_to_batch'')
,(''rcm'',''assign_remits''),(''common'',''carreport_AR''),(''common'',''casetypes_discount_factor'')
,(''rcm'',''check_reset_history''),(''common'',''Claim835Header''),(''common'',''Claim835Details'')
,(''common'',''Claim835ServiceDetails''),(''common'',''Claim835CASDetails'')
,(''common'',''Claim835AdditionalAdjustment''),(''common'',''claim_reason_master'')
,(''common'',''Claim835PLB''),(''common'',''Claim835SupplementalPayment''),(''common'',''cog_settings'')
,(''common'',''insurance_verification''),(''common'',''namematching''),(''common'',''namematchingpending'')
,(''rcm'',''patient_statements''),(''common'',''patients''),(''common'',''patients_notes'')
,(''rcm'',''payment_batches''),(''rcm'',''payment_plans''),(''rcm'',''payments'')
,(''rcm'',''pending_adjustment''),(''common'',''physicians''),(''common'',''post_payment_review'')
,(''common'',''PostedEOBBatchMapping''),(''common'',''ppr_document_notes''),(''common'',''Reference_Lab'')
,(''common'',''documents_scan_analysis''),(''rcm'',''EDINotes''),(''rcm'',''encounter_batch_detail'')
,(''rcm'',''encounter_reasons''),(''rcm'',''eob_patient''),(''rcm'',''eob_transaction'')
,(''rcm'',''EobPostingFiles''),(''rcm'',''expected_reimbursement_config''),(''common'',''ftr_remarks_master'')
,(''common'',''hl7_inbound''),(''common'',''hl7_tag_master''),(''common'',''DEFICIENCY_DENIAL_TABLE'')
,(''common'',''denial_carc_codes''),(''rcm'',''inserteob_from_service''),(''common'',''Inbound_data'')
,(''common'',''Inbound_data_log''),(''rcm'',''visit_hold_reasons''),(''rcm'',''weekly_report'')
,(''rcm'',''waystar_eligibility_response'');
SELECT d.ord AS load_order, d.SchemaName AS [schema], d.TableName AS [table],
  CASE WHEN OBJECT_ID(QUOTENAME(d.SchemaName)+''.''+QUOTENAME(d.TableName)) IS NULL THEN ''MISSING'' ELSE ''exists'' END AS table_status,
  ISNULL(rc.row_count,0) AS row_count,
  CASE WHEN ISNULL(rc.row_count,0)=0 THEN ''<-- EMPTY'' ELSE '''' END AS flag
FROM @Driver d
OUTER APPLY (SELECT SUM(ps.row_count) AS row_count FROM sys.dm_db_partition_stats ps WHERE ps.object_id=OBJECT_ID(QUOTENAME(d.SchemaName)+''.''+QUOTENAME(d.TableName)) AND ps.index_id IN (0,1)) rc
ORDER BY d.ord;";
    }

    private async Task<string> RunSqlPackageAsync(string arguments, CancellationToken ct,
        Action<string> onOutput)
    {
        var output = new StringBuilder();
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sqlpackage",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            process.Start();
            var readTask = Task.Run(() =>
            {
                while (!process.StandardOutput.EndOfStream && !ct.IsCancellationRequested)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (line != null)
                    {
                        output.AppendLine(line);
                        onOutput(line);
                    }
                }
                while (!process.StandardError.EndOfStream && !ct.IsCancellationRequested)
                {
                    var line = process.StandardError.ReadLine();
                    if (line != null)
                    {
                        output.AppendLine(line);
                        onOutput("[ERR] " + line);
                    }
                }
            }, ct);

            await Task.WhenAny(readTask, Task.Delay(-1, ct));
            if (!process.HasExited)
                process.Kill();
            process.WaitForExit(5000);
        }
        catch (Exception ex)
        {
            output.AppendLine($"sqlpackage error: {ex.Message}");
            onOutput($"Error: {ex.Message}");
        }
        return output.ToString();
    }
}
