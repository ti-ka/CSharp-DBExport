# CSharp-DBExport
This is a simple C# DB Exporter. This utility will copy your Database Tables as Models.

```
var settings = new ImportSettings
{
    ConnectionString = Constants.MasterConnectionString,
    Extend = Constants.BaseEntity,
    IgnoredColumns = (Constants.GetIgnoreColumnList()),
    NameSpace = Constants.MasterNamespaceLocation,
    OutputDirectory = Constants.MasterEntityOutputDirectory,
};
ExportEngine engine = new ExportEngine(settings);
engine.ExportTables();
```

If you wish to be maintainer of this project, email tika.pahadi@selu.edu.
