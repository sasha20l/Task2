using System.Xml.Linq;

var run = new Run();
run.ProcessClients();

public class Run
{
    public void ProcessClients()
    {
        var inputFile = "Clients.xml";         
        var validClientsFile = "valid_clients.xml";  
        var registratorsFile = "registrators.xml";   
        var errorsReportFile = "error_report.txt";  

        var clientsXml = XDocument.Load(inputFile);
        var clients = clientsXml.Descendants("Client").ToList();

        var validClients = new List<XElement>(); 
        var errors = new Dictionary<string, int>(); 
        var registrators = new Dictionary<string, int>(); 

        foreach (var client in clients)
        {
            var fio = client.Element("FIO")?.Value;
            var regNumber = client.Element("RegNumber")?.Value;
            var diasoftID = client.Element("DiasoftID")?.Value;
            var registrator = client.Element("Registrator")?.Value;

            var isValid = true;

            if (string.IsNullOrWhiteSpace(fio))
            {
                AddError(errors, "Не указано ФИО");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(regNumber))
            {
                AddError(errors, "Не указан Рег. номер");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(diasoftID))
            {
                AddError(errors, "Не указан DiasoftID");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(registrator))
            {
                AddError(errors, "Не указан Регистратор");
                isValid = false;
            }

            if (isValid)
            {
                if (!registrators.ContainsKey(registrator))
                {
                    registrators[registrator] = registrators.Count + 1;
                }
                var registratorID = registrators[registrator];
                client.Add(new XAttribute("RegistratorID", registratorID));
                validClients.Add(client);
            }
        }

        new XDocument(new XElement("Clients", validClients)).Save(validClientsFile);

        var registratorElements = registrators.Select(r =>
            new XElement("Registrator",
                new XElement("Name", r.Key),
                new XElement("ID", r.Value)
            )
        );
        new XDocument(new XElement("Registrators", registratorElements)).Save(registratorsFile);

        using (var writer = new System.IO.StreamWriter(errorsReportFile))
        {
            foreach (var error in errors.OrderByDescending(e => e.Value))
            {
                writer.WriteLine($"{error.Key}: {error.Value} записей");
            }
            writer.WriteLine($"Всего ошибочных записей: {errors.Values.Sum()}");
        }
    }

    static void AddError(Dictionary<string, int> errors, string error)
    {
        if (!errors.ContainsKey(error))
        {
            errors[error] = 0;
        }

        errors[error]++;
    }
}