create table dbo.BillingPeriodBillingStatus
(
    Id   smallint     not null
        primary key,
    Name nvarchar(50) not null
)
go

create table dbo.BillingPeriodStage
(
    Id   smallint     not null
        primary key,
    Name nvarchar(50) not null
)
go

create table dbo.BillingPeriod
(
    Id             bigint identity
        constraint BillingPeriod_pk
            primary key nonclustered,
    BillingEntityId     int                                      not null
        constraint FK_BillingPeriod_BillingEntityId
            references dbo.BillingEntity,
    PeriodStarting date                                     not null,
    PeriodEnding   date                                     not null,
    RowVersion     timestamp                                not null,
    Stage          smallint                                 not null
        constraint FK_BillingPeriod_Stage
            references dbo.BillingPeriodStage,
    BillingStatus  smallint                                 not null
        constraint FK_BillingPeriod_BillingStatus
            references dbo.BillingPeriodBillingStatus
)
go

alter table dbo.BillingPeriod
    add constraint DF_BillingPeriod_BillingStatus default 0 for BillingStatus
go

insert into dbo.BillingPeriodBillingStatus (Id, Name)
values  (-1, N'Failed'),
        (0, N'Not processed'),
        (3, N'Completed'),
        (4, N'Processing')
go

create index IX_BillingPeriod_BillingEntityIdPeriodEnding
    on dbo.BillingPeriod (BillingEntityId, PeriodEnding)
go

create index IX_BillingPeriod_PeriodEnding
    on dbo.BillingPeriod (PeriodEnding)
go

create index IX_BillingPeriod_PeriodStarting
    on dbo.BillingPeriod (PeriodStarting)
go

create index IX_BillingPeriod_BillingStatus
    on dbo.BillingPeriod (BillingStatus)
go

create index IX_BillingPeriod_Stage
    on dbo.BillingPeriod (Stage)
go

insert into dbo.BillingPeriodStage (Id, Name)
values  (0, N'Period in progress'),
        (20, N'Record billables'),
        (40, N'Post Customer invoice'),
        (50, N'Post vendor bill'),
        (60, N'Settle vendor bill'),
        (100, N'Done')

go
