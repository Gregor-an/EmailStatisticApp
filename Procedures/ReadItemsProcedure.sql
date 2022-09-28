CREATE PROCEDURE dbo.ReadEmailInfo 
AS 
     SET NOCOUNT ON;

     SELECT TOP (100)
            [Id],
            [Sender],
            [Subject],
            [From],
            [To],
            [MessageId],
            [TimeOfExecution]  
     FROM [dbo].[EmailsInfo] ORDER BY [TimeOfExecution] DESC;
     RETURN; 
GO