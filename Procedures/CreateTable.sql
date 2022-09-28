CREATE TABLE EmailsInfo (
    Id int NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Sender varchar(255),
    Subject varchar(255),
    MessageId varchar(255),
    TimeOfExecution datetime
);