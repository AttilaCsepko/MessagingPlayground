CREATE TABLE [dbo].[Messages](  
    [ID] [int] IDENTITY(1,1) NOT NULL,  
    [UUID] [nvarchar](100) NOT NULL,  
    [Source] [nvarchar](100) NOT NULL,  
    [MsgJson] [nvarchar] (max) NULL,  
 CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED   
(  
    [ID] ASC  
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]  
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]  
GO 

ALTER TABLE [dbo].[Messages] WITH CHECK ADD CONSTRAINT [MsgJson record should be formatted as JSON] CHECK ((isjson([MsgJson])=(1)))
GO

ALTER TABLE  [dbo].[Messages] CHECK CONSTRAINT [MsgJson record should be formatted as JSON]
GO