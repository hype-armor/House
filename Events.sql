USE [Echo]
GO

/****** Object:  Table [dbo].[Events]    Script Date: 3/2/2016 9:57:03 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Events](
	[EventID] [int] IDENTITY(1,1) NOT NULL,
	[ClientID] [int] NOT NULL,
	[RecurringType] [int] NOT NULL,
	[TriggerDateTime] [datetime] NOT NULL,
	[Message] [varchar](100) NULL,
	[Action] [varbinary](max) NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Events]  WITH CHECK ADD  CONSTRAINT [FK_Events_Clients] FOREIGN KEY([ClientID])
REFERENCES [dbo].[Clients] ([ClientID])
GO

ALTER TABLE [dbo].[Events] CHECK CONSTRAINT [FK_Events_Clients]
GO


