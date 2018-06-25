﻿SELECT 

ORDINAL_POSITION AS [Index],
TABLE_SCHEMA [Schema],
TABLE_NAME AS [TableName],
COLUMN_NAME AS [ColumnName],
DATA_TYPE AS [SqlDataType],
IIF(IS_NULLABLE = 'YES',1,0) AS IsNullable,
COLUMNPROPERTY(object_id(TABLE_SCHEMA+'.'+TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS [IsIdentity],
CHARACTER_MAXIMUM_LENGTH AS [MaxLength]
 
FROM INFORMATION_SCHEMA.COLUMNS


WHERE 
     TABLE_SCHEMA = @0
	 AND
     TABLE_NAME = @1
	 
ORDER BY ORDINAL_POSITION