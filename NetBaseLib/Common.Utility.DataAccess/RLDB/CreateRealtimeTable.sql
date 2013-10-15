USE [EcommerceRealtime]
GO

CREATE TABLE [dbo].[RealTimeData](
	[SysNo] [int] IDENTITY(1,1) NOT NULL,
	[BusinessKey] [varchar](100) NOT NULL,
	[BusinessDataType] [varchar](500) NOT NULL,
	[ChangeType] [char](1) NOT NULL,
	[BusinessData] [xml] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RealTimeData] PRIMARY KEY CLUSTERED 
(
	[SysNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


CREATE TABLE [dbo].[RealTimeData_History](
	[SysNo] [int] IDENTITY(1,1) NOT NULL,
	[BusinessKey] [varchar](100) NOT NULL,
	[BusinessDataType] [varchar](500) NOT NULL,
	[ChangeType] [char](1) NOT NULL,
	[BusinessData] [xml] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RealTimeData_History] PRIMARY KEY CLUSTERED 
(
	[SysNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


