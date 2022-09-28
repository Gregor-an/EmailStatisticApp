CREATE PROCEDURE dbo.InsertEmailInfo 
       @Sender                      NVARCHAR(255)   = NULL   , 
       @Subject                     NVARCHAR(255)   = NULL   ,
       @From                        NVARCHAR(255)   = NULL   ,
       @To                          NVARCHAR(255)   = NULL   ,
       @MessageId                   NVARCHAR(255)   = NULL   , 
       @TimeOfExecution             DATETIME        = NULL  
AS 
BEGIN 
     SET NOCOUNT ON 

     INSERT INTO [dbo].[EmailsInfo]
          (                    
            [Sender],
            [Subject],
            [From],
            [To],
            [MessageId],
            [TimeOfExecution]                 
          ) 
     VALUES 
          ( 
            @Sender,
            @Subject,
            @From,
            @To,
            @MessageId,
            @TimeOfExecution
          ) 

END 

GO