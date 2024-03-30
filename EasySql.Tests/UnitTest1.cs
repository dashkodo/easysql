using FluentAssertions;

namespace EasySql.Tests;

public class FormatterTests
{
    private static string dropConstaint = """
        alter table SampleTable
        drop
            constraint
                PK_SampleTable,
                UQ_SampleTable_DashboardType_SortOrder
        """;
    private static string addConstaint = """
        alter table SampleTable
        add constraint UQ_SampleTable_ResourceId unique (ResourceId)
        with (fillfactor = 90, data_compression = page)
        """;
    private static string addColumns = """
        alter table SampleTable
        add
            SampleTableId int not null identity
                constraint PK_SampleTable primary key with (fillfactor = 100),
            Enabled bit not null
                constraint DF_SampleTable_Enabled default 1,
            StartedUtcDateTime datetime2(2) generated always as row start not null  
                constraint DF_SampleTable_StartedUtcDateTime default '20000101',
            EndedUtcDateTime datetime2(2) generated always as row end not null            
                constraint DF_SampleTable_EndedUtcDateTime default '99991231 23:59:59.9999999',
            period for system_time(StartedUtcDateTime, EndedUtcDateTime),
            CreatedUtcDateTime datetime2(2) not null
                constraint DF_SampleTable_CreatedUtcDateTime default ('20000101'),
            CreatedByUserId int null
                constraint FK_SampleTable_CreatedByUserId foreign key references "User" (Id),
            LastUpdatedByUserId int null
                constraint FK_SampleTable_LastUpdatedByUserId foreign key references "User" (Id)
        """;
    private static string declare = """
        declare @CreatedByUserId int = (select UserId from SystemUser where UserType = 'TEST')
        """;
        
    private static string update = """
        update SampleTable
        set
            CreatedByUserId = @CreatedByUserId,
            LastUpdatedByUserId = @CreatedByUserId
        """;
        
    private static string alterColumn = """
        alter table SampleTable
        alter column CreatedByUserId int not null
        """;
       
    private static string setSystemVersioning = """
        alter table SampleTable
        set (system_versioning = on (history_table = dbo.SampleTableHistory, data_consistency_check = on))
        """;
    private static string exec = """
        exec sp_rename 'SampleTableHistory.ix_SampleTableHistory', 'IX_SampleTableHistory_EndedUtcDateTime_StartedUtcDateTime', 'INDEX'
        """;
        
    private static string createTable = """
        create table SampleTableCulture
        (
            SampleTableId int not null
                constraint FK_SampleTableCulture_SampleTableId references SampleTable(SampleTableId),
            CultureId int not null
                constraint FK_SampleTableCulture_CultureId references Culture(Id),
            ResourceId uniqueidentifier not null,
            Deleted bit not null
                constraint DF_SampleTableCulture_Deleted default (0),
            CreatedByUserId int not null
                constraint FK_SampleTableCulture_CreatedByUserId foreign key (CreatedByUserId) references "User"(Id),
            CreatedUtcDateTime datetime2(2) not null
                constraint DF_SampleTableCulture_CreatedUtcDateTime default (sysutcdatetime()),
            LastUpdatedByUserId int not null
                constraint FK_SampleTableCulture_LastUpdatedByUserId foreign key(LastUpdatedByUserId) references "User"(Id),
            StartedUtcDateTime datetime2(2) generated always as row start not null
                constraint DF_SampleTableCulture_StartedUtcDateTime default '20000101',
            EndedUtcDateTime datetime2(2) generated always as row end not null
                constraint DF_SampleTableCulture_EndedUtcDateTime default '99991231 23:59:59.9999999',
            constraint PK_SampleTableCulture primary key clustered (SampleTableId, CultureId) with (data_compression = page, fillfactor = 90),
            period for system_time(StartedUtcDateTime, EndedUtcDateTime)
        )
        with (system_versioning = on (history_table = dbo.SampleTableCultureHistory, data_consistency_check = on))
        """;
        
    private static string createIndex = """
        create unique index FUX_SampleTableCulture_ResourceId
        on SampleTableCulture(ResourceId)
        where Deleted != 1
        with (fillfactor = 90, data_compression = page)
        """;
        
    private static string insert = """
        insert SampleTableCulture
        (
            SampleTableId, 
            CultureId, 
            ResourceId, 
            CreatedByUserId,
            LastUpdatedByUserId
        )
        select
            SampleTableId,
            1033, -- en-US
            ResourceId,
            @CreatedByUserId,
            @CreatedByUserId
        from SampleTable
        """;
    
    private static string sample = """
        alter table SampleTable
        drop
            constraint
                PK_SampleTable,
                UQ_SampleTable_DashboardType_SortOrder
        
        alter table SampleTable
        add constraint UQ_SampleTable_ResourceId unique (ResourceId)
        with (fillfactor = 90, data_compression = page)
        
        alter table SampleTable
        add
            SampleTableId int not null identity
                constraint PK_SampleTable primary key with (fillfactor = 100),
            Enabled bit not null
                constraint DF_SampleTable_Enabled default 1,
            StartedUtcDateTime datetime2(2) generated always as row start not null  
                constraint DF_SampleTable_StartedUtcDateTime default '20000101',
            EndedUtcDateTime datetime2(2) generated always as row end not null            
                constraint DF_SampleTable_EndedUtcDateTime default '99991231 23:59:59.9999999',
            period for system_time(StartedUtcDateTime, EndedUtcDateTime),
            CreatedUtcDateTime datetime2(2) not null
                constraint DF_SampleTable_CreatedUtcDateTime default ('20000101'),
            CreatedByUserId int null
                constraint FK_SampleTable_CreatedByUserId foreign key references "User" (Id),
            LastUpdatedByUserId int null
                constraint FK_SampleTable_LastUpdatedByUserId foreign key references "User" (Id)
        go
        
        declare @CreatedByUserId int = (select UserId from SystemUser where UserType = 'TEST')
        
        update SampleTable
        set
            CreatedByUserId = @CreatedByUserId,
            LastUpdatedByUserId = @CreatedByUserId
        
        alter table SampleTable
        alter column CreatedByUserId int not null
        
        alter table SampleTable
        alter column LastUpdatedByUserId int not null
        
        alter table SampleTable
        set (system_versioning = on (history_table = dbo.SampleTableHistory, data_consistency_check = on))
        
        exec sp_rename 'SampleTableHistory.ix_SampleTableHistory', 'IX_SampleTableHistory_EndedUtcDateTime_StartedUtcDateTime', 'INDEX'
        
        create table SampleTableCulture
        (
            SampleTableId int not null
                constraint FK_SampleTableCulture_SampleTableId references SampleTable(SampleTableId),
            CultureId int not null
                constraint FK_SampleTableCulture_CultureId references Culture(Id),
            ResourceId uniqueidentifier not null,
            Deleted bit not null
                constraint DF_SampleTableCulture_Deleted default (0),
            CreatedByUserId int not null
                constraint FK_SampleTableCulture_CreatedByUserId foreign key (CreatedByUserId) references "User"(Id),
            CreatedUtcDateTime datetime2(2) not null
                constraint DF_SampleTableCulture_CreatedUtcDateTime default (sysutcdatetime()),
            LastUpdatedByUserId int not null
                constraint FK_SampleTableCulture_LastUpdatedByUserId foreign key(LastUpdatedByUserId) references "User"(Id),
            StartedUtcDateTime datetime2(2) generated always as row start not null
                constraint DF_SampleTableCulture_StartedUtcDateTime default '20000101',
            EndedUtcDateTime datetime2(2) generated always as row end not null
                constraint DF_SampleTableCulture_EndedUtcDateTime default '99991231 23:59:59.9999999',
            constraint PK_SampleTableCulture primary key clustered (SampleTableId, CultureId) with (data_compression = page, fillfactor = 90),
            period for system_time(StartedUtcDateTime, EndedUtcDateTime)
        )
        with (system_versioning = on (history_table = dbo.SampleTableCultureHistory, data_consistency_check = on))
        
        exec sp_rename 'SampleTableCultureHistory.ix_SampleTableCultureHistory', 'IX_SampleTableCultureHistory_EndedUtcDateTime_StartedUtcDateTime', 'INDEX'
        go
        
        create unique index FUX_SampleTableCulture_ResourceId
        on SampleTableCulture(ResourceId)
        where Deleted != 1
        with (fillfactor = 90, data_compression = page)
        
        declare @CreatedByUserId int = (select UserId from SystemUser where UserType = 'SEED')
        
        insert SampleTableCulture
        (
            SampleTableId, 
            CultureId, 
            ResourceId, 
            CreatedByUserId,
            LastUpdatedByUserId
        )
        select
            SampleTableId,
            1033, -- en-US
            ResourceId,
            @CreatedByUserId,
            @CreatedByUserId
        from SampleTable
        """;

    public static IEnumerable<TestCaseData> ShouldKeepFormattingSource()
    {
        return new[]
        {
            dropConstaint,
            addConstaint,
            addColumns,
            declare,
            update,
            alterColumn,
            setSystemVersioning,
            createIndex,
            createTable,
            insert,
            exec
        }.Select(arg => new TestCaseData(arg));
    }
    
    [TestCaseSource(nameof(ShouldKeepFormattingSource))]
    public void ShouldKeepFormatting(string input)
    {
        new Formatter(input.TrimStart('\n')).Format().Should().Be(input);
    }
}