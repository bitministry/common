
CREATE TABLE WindowsEvent (
    id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    host          VARCHAR(32)   NOT NULL,
    log_name      NVARCHAR(16)    NOT NULL,   -- Application/System/Security
    source        NVARCHAR(256)   NOT NULL,   -- ProviderName
    event_id      INT             NOT NULL,
    level         INT             NULL,       -- 1..5 or NULL
    time_created  DATETIME2(0)    NOT NULL,
    message       NVARCHAR(MAX)   NULL
);

-- unique key used by your Python upsert id_cols
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'UX_WindowsEvents_LogicalKey')
CREATE UNIQUE INDEX UX_WindowsEvents_LogicalKey
ON dbo.WindowsEvents(host, log_name, source, event_id, time_created);