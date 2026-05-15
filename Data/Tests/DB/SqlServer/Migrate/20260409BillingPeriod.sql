create table dbo.BillingPeriodBillingStatuses
(
    Id   smallint     not null
        primary key,
    Name nvarchar(50) not null
)
go

create table dbo.BillingPeriodStages
(
    Id   smallint     not null
        primary key,
    Name nvarchar(50) not null
)
go

create table dbo.BillingPeriods
(
    Id             bigint identity
        constraint BillingPeriods_pk
            primary key nonclustered,
    PartnerId     int                                      not null
        constraint FK_BillingPeriods_PartnerId
            references dbo.Partner,
    PeriodStarting date                                     not null,
    PeriodEnding   date                                     not null,
    RowVersion     timestamp                                not null,
    Stage          smallint                                 not null
        constraint FK_BillingPeriods_Stage
            references dbo.BillingPeriodStages,
    BillingStatus  smallint                                 not null
        constraint FK_BillingPeriods_BillingStatus
            references dbo.BillingPeriodBillingStatuses
)
go

alter table dbo.BillingPeriods
    add constraint DF_BillingPeriods_BillingStatus default 0 for BillingStatus
go

create index IX_BillingPeriods_PartnerIdPeriodEnding
    on dbo.BillingPeriods (PartnerId, PeriodEnding)
go

create index IX_BillingPeriods_PeriodEnding
    on dbo.BillingPeriods (PeriodEnding)
go

create index IX_BillingPeriods_PeriodStarting
    on dbo.BillingPeriods (PeriodStarting)
go

create index IX_BillingPeriods_BillingStatus
    on dbo.BillingPeriods (BillingStatus)
go

create index IX_BillingPeriods_Stage
    on dbo.BillingPeriods (Stage)
go

create table dbo.DataTypes
(
    Id   smallint     not null
        primary key,
    Name nvarchar(50) not null
)
go

insert into dbo.DataTypes (Id, Name)
values  (0, N'String'),
        (1, N'Guid'),
        (2, N'DateTime'),
        (3, N'DateTimeOffset'),
        (4, N'Int64');
go

create table dbo.BillingPeriodProperties
(
    Id   bigint identity
        primary key,
    BillingPeriodId bigint          not null
        constraint FK_BillingPeriodProperties_BillingPeriod
            references dbo.BillingPeriods
            on delete cascade,
    PropertyName    nvarchar(50) not null,
    DataType        smallint     not null
        constraint FK_BillingPeriodProperties_DataType
            references dbo.DataTypes,
    DataValue       nvarchar(max)
)
go

create unique index IX_BillingPeriodProperties_BillingPeriodId_PropertyName
    on dbo.BillingPeriodProperties (BillingPeriodId, PropertyName)
go

create table dbo.BillingPeriodTaskStatuses
(
    Id          smallint     not null
        constraint PK_BillingPeriodTaskStatus
            primary key,
    Description varchar(200) not null
)
go

create table dbo.BillingPeriodTaskTypes
(
    Id          smallint     not null
        constraint PK_BillingPeriodTaskType
            primary key,
    Description varchar(100) not null
)
go

create table dbo.BillingPeriodTasks
(
    Id   bigint identity (1, 1)
        constraint PK_BillingPeriodTask2
            primary key nonclustered,
    BillingPeriodId bigint                        not null
        constraint FK_BillingPeriodTasks_BillingPeriod
            references dbo.BillingPeriods
            on delete cascade,
    TaskType            smallint                   not null
        constraint FK_BillingPeriodTasks_BillingPeriodTaskType
            references dbo.BillingPeriodTaskTypes,
    Status          smallint                   not null
        constraint FK_BillingPeriodTasks_BillingPeriodTaskStatus
            references dbo.BillingPeriodTaskStatuses,
    DependsOn       varchar(1000)                          not null,
    CreatedAt       datetimeoffset                         not null,
    LastAttemptedAt datetimeoffset,
    WaitUntil       datetimeoffset,
    Data            nvarchar(max),
    Stage           smallint                   not null
        constraint FK_BillingPeriodTasks_Stage
            references dbo.BillingPeriodStages
)
go

alter table dbo.BillingPeriodTasks
    add constraint DF_BillingPeriodTasks_DependsOn default '[]' for DependsOn
go

create index IX_BillingPeriodTasks_Status
    on dbo.BillingPeriodTasks (Status)
go

create index IX_BillingPeriodTasks_WaitUntil
    on dbo.BillingPeriodTasks (WaitUntil)
go

create index IX_BillingPeriodTasks_Type
    on dbo.BillingPeriodTasks (TaskType)
go

insert into dbo.BillingPeriodTaskStatuses (Id, Description)
values  (-2, N'Failed'),
        (-1, N'Failed (to be retried)'),
        (0, N'New'),
        (2, N'Succeeded');

go

insert into dbo.BillingPeriodStages (Id, Name)
values  (0, N'Period in progress'),
        (20, N'Record billables'),
        (40, N'Post Customer invoice'),
        (50, N'Post vendor bill'),
        (60, N'Settle vendor bill'),
        (100, N'Done')

go

insert into dbo.BillingPeriodTaskTypes (Id, Description)
values  (20, N'Refresh Sales Fees and Commission Totals by Billing Code'),
        (40, N'Create EoBP Sage ARCustomerInvoice'),
        (41, N'Post EoBP Sage ARCustomerInvoice'),
        (50, N'Create EoBP Sage APVendorInvoice'),
        (51, N'Post EoBP Sage APVendorInvoice'),
        (60, N'Create EoBP Sage ARCustomerCreditNote'),
        (61, N'Post EoBP Sage ARCustomerCreditNote'),
        (62, N'Create EoBP Sage ARReceiptAndAdjustment'),
        (63, N'Post EoBP Sage ARReceiptAndAdjustment'),
        (64, N'Create EoBP Sage APVendorCreditNote'),
        (65, N'Post EoBP Sage APVendorCreditNote'),
        (66, N'Create EoBP Sage APPaymentAndAdjustment'),
        (67, N'Post EoBP Sage APPaymentAndAdjustment')
go
