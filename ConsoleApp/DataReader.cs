namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport) // bool printData = true , też nie wymagane domyślnie jest true
        {
            ImportedObjects = new List<ImportedObject>();       // inicjalizacja nie jest obowiązkowa : { new ImportedObject()}

            var streamReader = new StreamReader(fileToImport);

            var importedLines = new List<string>();
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                importedLines.Add(line);
            }

            for (int i = 0; i < importedLines.Count; i++) // zniana z <= na < aby uniknąć 'IndexOutOfRangeException'
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';');
                if (values.Length < 6) // Sprawdza, czy linia ma wystarczającą liczbę danych, jak nie pomija
                {
                    Console.WriteLine($"Linia {i + 1} jest niekompletna i zostanie pominięta.");
                    continue;
                }
                var importedObject = new ImportedObject();
                importedObject.Type = values[0];
                importedObject.Name = values[1];
                importedObject.Schema = values[2];
                importedObject.ParentName = values[3];
                importedObject.ParentType = values[4];
                importedObject.DataType = values[5];
                // Sprawdza, czy istnieje opcjonalny szósty element; jeśli nie, przypisuje domyślną wartość
                importedObject.IsNullable = values.Length > 6 ? values[6] : "0";  //"0" oznacza "nie jest nullable"
                ((List<ImportedObject>)ImportedObjects).Add(importedObject);
            }
            string CleanString(string input)
            {
                return input?.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper() ?? string.Empty;
                //zmniejszenie ilości kodu poniżej
            }

            // clear and correct imported data
            foreach (var importedObject in ImportedObjects)
            {
                importedObject.Type = CleanString(importedObject.Type);
                importedObject.Name = CleanString(importedObject.Name);
                importedObject.Schema = CleanString(importedObject.Schema);
                importedObject.ParentName = CleanString(importedObject.ParentName);
                importedObject.ParentType = CleanString(importedObject.ParentType);
            }

            // assign number of children
            for (int i = 0; i < ImportedObjects.Count(); i++)
            {
                var importedObject = ImportedObjects.ToArray()[i];
                foreach (var impObj in ImportedObjects)
                {
                    if (impObj.ParentType == importedObject.Type)
                    {
                        if (impObj.ParentName == importedObject.Name)
                        {
                            importedObject.NumberOfChildren = 1 + importedObject.NumberOfChildren;
                        }
                    }
                }
            }

            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    // print all database's tables
                    foreach (var table in ImportedObjects)
                    {
                        if (table.ParentType.ToUpper() == database.Type)
                        {
                            if (table.ParentName == database.Name)
                            {
                                Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                                // print all table's columns
                                foreach (var column in ImportedObjects)
                                {
                                    if (column.ParentType.ToUpper() == table.Type)
                                    {
                                        if (column.ParentName == table.Name)
                                        {
                                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }

    class ImportedObject : ImportedObjectBaseClass
    {
        public string Name
        {
            get;
            set;
        }
        public string Schema;

        public string ParentName;
        public string ParentType
        {
            get; set;
        }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public double NumberOfChildren;
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
