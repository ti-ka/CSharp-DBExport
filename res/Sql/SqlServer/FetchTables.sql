SELECT

sch.name as [Schema],
so.name as [TableName]

FROM
  sys.sysobjects so
  
LEFT JOIN sys.schemas sch  ON so.uid = sch.schema_id 

WHERE so.xtype in ('U', 'V')