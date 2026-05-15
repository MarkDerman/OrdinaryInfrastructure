create table dbo.PartnerStatuses
(
    Id   smallint     not null
        constraint PK_PartnerStatuses
            primary key,
    Description nvarchar(100) not null
)
go

insert into dbo.PartnerStatuses (Id, Description)
values  (0, N'Not active'),
        (1, N'Active')
go

create table dbo.Partner
(
    Id                             int identity
        constraint Partner_PK
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
        constraint FK_Partner_PartnerStatuses
            references dbo.PartnerStatuses
)
go

create index IX_Partner_Name
    on dbo.Partner (Name)
go

create index IX_Partner_BillingName
    on dbo.Partner (BillingName)
go

create index IX_Partner_SageARCustomerNumber
    on dbo.Partner (SageARCustomerNumber)
go

create index IX_Partner_SageAPVendorNumber
    on dbo.Partner (SageAPVendorNumber)
go

create index IX_Partner_Status
    on dbo.Partner (Status)
go

create index IX_Partner_SageIntegratedFrom
    on dbo.Partner (SageIntegratedFrom)
go

create index IX_Partner_VatNumber
    on dbo.Partner (VatNumber)
go


