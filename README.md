# Projeto

Essa aplicação consiste em um programa de console em C# que verifica a cotação de ativos da B3 e envia alertas por email dando um sinal para venda ou compra caso o preço do ativo esteja acima ou abaixo de um certo limite.

## Características
- O programa fica continuamente monitorando a cotação do ativo enquanto estiver rodando;
- Faz o envio de alertas por email sempre que atingir os limites estipulados;

## Preparação do ambiente
- **.NET SDK** instalado.
   - [Download .NET SDK](https://dotnet.microsoft.com/download)
- Servidor SMTP (Gmail).

## Configuração

Na raiz do projeto há um arquivo de configuração nomeado como "config.txt", nele há informações como:

```plaintext
SMTPServer=smtp.gmail.com
SMTPPort=587
SMTPUser=seu_email@gmail.com
SMTPPassword=sua_senha 
EmailRecipient=destinatario@gmail.com
ApiToken=seu_token_da_api 
```

No caso do Gmail, certifique-se de gerar uma senha de aplicativo para segurança. Além disso, o token da API foi disponibilizado para uso por tempo determinado então, caso necessário, considere adquirir credenciais da API para que seja possível fazer consultas de valores das cotações.

## Uso

1. Navegue até a pasta do projeto no seu terminal e execute
```plaintext
dotnet build
```
2. Gere o executável 
```plaintext
dotnet publish -c Release -r win-x64 --self-contained false
```
Ele será gerando em bin/Release/net6.0/win-x64/

3. Execute o Programa
Navegue até a pasta onde foi gerado o executável pelo terminal e execute
```plaintext
.\stock-quote-alert.exe <ativo> <preço_de_referencia_para_venda> <preço_de_referencia_para_compra>
```
Exemplo:
```plaintext
.\stock-quote-alert.exe PETR4 22.67 22.59 
```

## Dependências
Certifique-se de possuir o pacote Newtonsoft.Json instalado
```plaintext
dotnet add package Newtonsoft.Json
```


