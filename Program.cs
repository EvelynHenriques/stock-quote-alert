using System; 
using System.IO; 
using System.Globalization; 
using System.Net; 
using System.Net.Mail; 
using System.Threading; 
using Newtonsoft.Json.Linq; 


class Program{
    static async Task Main(string[] args){
        if (args.Length != 3){
            Console.WriteLine("Faça o uso corretamente: stock-quote-alert.exe <ativo> <preço_de_referencia_para_venda> <preço_de_referencia_para_compra>");
            return;
        }

        string ativo = args[0];
        decimal salePrice = Convert.ToDecimal(args[1], CultureInfo.InvariantCulture);
        decimal purchPrice = Convert.ToDecimal(args[2], CultureInfo.InvariantCulture);
                
        var config = SMTPConfiguration.ReadFile("config.txt");
        
        if(config == null){
            Console.WriteLine("Erro no arquivo de configuração");
            return;
        }

        var emailService = new EmailService(config);
        var stockService = new StockPriceService(config.ApiToken);

        Console.WriteLine($"\nMonitorando o ativo {ativo} considerando: \nPreço para venda: {salePrice}\nPreço para compra: {purchPrice}\n");
        //LoadingBar(60000,6);

        while (true)
        {
            try{

                decimal precoAtual = await stockService.FindPrice(ativo);

                if (precoAtual > salePrice)
                {
                    await emailService.sendEmail($"Ação {ativo} - Alerta para venda",
                        $"A cotação do ativo {ativo} subiu para {precoAtual}. Aproveite para vender!");
                }
                else if (precoAtual < purchPrice)
                {
                    await emailService.sendEmail($"Ação {ativo} - Alerta para compra",
                        $"A cotação do ativo {ativo} caiu para {precoAtual}. Aproveite para comprar!");
                
                }else{
                    Console.WriteLine($"Não há sinais para venda ou compra desse ativo a partir dos níveis estipulados.");
                }
            }
            catch(Exception e){
                Console.WriteLine($"Erro no monitoramento do ativo {ativo}: {e.Message}");
    
            }
            //Thread.Sleep(60000);
            Console.WriteLine($"\nAguardando 30s para nova consulta.");
            await LoadingBar(30000,20);
            //await Task.Delay(60000);

        }
    }
    static async Task LoadingBar(int time, int steps){
        int interval = time/steps;
        string bar = new string('-',steps);
        //Console.WriteLine(bar);
        

        for(int i = 0; i<steps;i++){
            bar = bar.Remove(i,1);
            bar = bar.Insert(i,"#");
            Console.Write($"\r[{bar}]");
            
            await Task.Delay(interval);
        }
    }
}

class SMTPConfiguration{
    public string smtpServer{ get; set;}
    public int smtpPort{ get; set;}
    public string smtpUser{ get; set;}
    public string smtpPassword{ get; set;}
    public string emailRecipient{ get; set;}
    public string ApiToken { get; set;}

    public static SMTPConfiguration? ReadFile(string file){

        if(!File.Exists(file)){
            Console.WriteLine("O arquivo de configuração de email não foi encontrado.");
            return null;
        }

        var config = new SMTPConfiguration();
        foreach(var line in File.ReadAllLines(file)){
            var parts = line.Split('=');
            if(parts.Length!=2) continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            switch(key)
            {
                case "SMTPServer":
                    config.smtpServer = value;
                    break;
                case "SMTPPort":
                    config.smtpPort = int.Parse(value);
                    break;
                case "SMTPUser":
                    config.smtpUser = value;
                    break;
                case "SMTPPassword":
                    config.smtpPassword = value;
                    break;
                case "EmailRecipient":
                    config.emailRecipient = value;
                    break;
                case "ApiToken":
                    config.ApiToken = value;
                    break;
            }
        }
        return config;
    }

}

class EmailService{
    private readonly SMTPConfiguration conf;

    public EmailService(SMTPConfiguration config){
        conf = config;
    }

    public async Task sendEmail(string subject, string message){
        try{
            using var client = new SmtpClient(conf.smtpServer, conf.smtpPort){ 
                Credentials = new NetworkCredential(conf.smtpUser, conf.smtpPassword), 
                EnableSsl = true                                                                
            };                                                                         

            var mailMessage = new MailMessage
            {
                //From = conf.smtpUser,
                From = new MailAddress(conf.smtpUser),
                Subject = subject,
                Body = message
                //To = conf.emailRecipient
            };

            mailMessage.To.Add(conf.emailRecipient); 

            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"Email enviado! \nVerifique a caixa de mensagens com o seguinte assunto: {subject}");
        }
        catch(Exception e){
            Console.WriteLine($"Erro no envio do email: {e.Message}.");
        }
    }
}

class StockPriceService{
    private static readonly HttpClient client = new HttpClient();
    private const string apiEndpoint = "https://brapi.dev/api";
    private readonly string apiToken;

    public StockPriceService(string token){
        apiToken = token;
    }

    public async Task<decimal> FindPrice(string ativo)
    {
        string url = $"{apiEndpoint}/quote/{ativo}?token={apiToken}";
        //Console.WriteLine($"Realizando consulta na API: {url}");

        try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                //Console.WriteLine($"response:\n{response}");

                if(response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(responseBody);

                    JObject json = JObject.Parse(responseBody);
                    //Console.WriteLine(json);
                    
                    decimal price = (decimal)json["results"][0]["regularMarketPrice"];
                    Console.WriteLine($"O preço da cotação {ativo} hoje é: {price}\n");
                    return price;
                }
                else
                {
                    Console.WriteLine($"Erro na requisição HTTP: {response.StatusCode}");
                    return 0;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Erro durante a consulta do preço do ativo {ativo}: {e.Message}");
                return 0;
            }
    }
}
