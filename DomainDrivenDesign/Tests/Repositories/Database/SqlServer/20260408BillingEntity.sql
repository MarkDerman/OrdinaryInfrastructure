create table dbo.BillingEntityStatus
(
    Id   smallint     not null
        constraint PK_BillingEntityStatus
            primary key,
    Description nvarchar(100) not null
)
go

insert into dbo.BillingEntityStatus (Id, Description)
values  (0, N'Not active'),
        (1, N'Active')
go

create table dbo.BillingEntity
(
    Id                             int identity
        constraint BillingEntity_PK
            primary key nonclustered,
    Name                           nvarchar(120) not null,
    BillingName                    nvarchar(120),
    BillingAddress                 nvarchar(500),
    VatNumber                      nvarchar(50),
    SageARCustomerNumber           nvarchar(12),
    SageAPVendorNumber             nvarchar(12),
    SageIntegratedFrom             date,
    BillingCycleJson               nvarchar(1000),
    Status                         smallint not null
        constraint FK_BillingEntity_BillingEntityStatus
            references dbo.BillingEntityStatus
)
go

create index IX_BillingEntity_Name
    on dbo.BillingEntity (Name)
go

create index IX_BillingEntity_BillingName
    on dbo.BillingEntity (BillingName)
go

create index IX_BillingEntity_SageARCustomerNumber
    on dbo.BillingEntity (SageARCustomerNumber)
go

create index IX_BillingEntity_SageAPVendorNumber
    on dbo.BillingEntity (SageAPVendorNumber)
go

create index IX_BillingEntity_Status
    on dbo.BillingEntity (Status)
go

create index IX_BillingEntity_SageIntegratedFrom
    on dbo.BillingEntity (SageIntegratedFrom)
go

create index IX_BillingEntity_VatNumber
    on dbo.BillingEntity (VatNumber)
go


