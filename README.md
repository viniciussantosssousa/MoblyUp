# MoblyUp

Marketplace que conecta clientes que precisam montar móveis a montadores profissionais da mesma cidade. O cliente publica o pedido, os montadores da região enviam propostas e o cliente escolhe, paga e recebe a foto da montagem concluída.

| Pasta | O que é |
|---|---|
| `MoblyUpApp/` | Aplicativo em React Native (Expo SDK 54) |
| `Projeto-MoblyUp/` | API em C# (.NET 10, Clean Architecture, SQL Server) |
| `Projeto-MoblyUp/tests/` | Testes automatizados das regras de negócio (xUnit) |

## Como rodar

São necessários o .NET 10 SDK, o Node.js e o SQL Server local (a conexão fica em `Projeto-MoblyUp/src/MoblyUp.API/appsettings.json`).

**1. API** (em um terminal):

```bash
cd Projeto-MoblyUp/src/MoblyUp.API
dotnet ef database update --project ../MoblyUp.Infrastructure
dotnet run --launch-profile http
```

A API sobe em `http://0.0.0.0:5007`, acessível pela rede local. A documentação Swagger fica em `http://localhost:5007/swagger`.

**2. Aplicativo** (em outro terminal):

```bash
cd MoblyUpApp
npm install
npx expo start
```

Aperte `w` para abrir no navegador ou escaneie o QR code com o Expo Go. Para usar no celular, copie `MoblyUpApp/.env.example` para `.env` e coloque o IP do computador que roda a API.

**3. Testes:**

```bash
dotnet test Projeto-MoblyUp/tests/MoblyUp.Tests
```

## Jornada do pedido

```
Disponível ──proposta──▶ Aguardando aprovação ──aceite──▶ Aguardando pagamento ──pagamento──▶ Em andamento ──foto + conclusão──▶ Concluído
    ▲                         │        │                        │
    └──── recusa do cliente ──┘        └── desistência do montador (antes do pagamento)
Cancelado: o cliente pode cancelar enquanto não pagou.
```

- **Taxa da plataforma:** 15% de cada proposta, calculada na API (ex.: R$ 100 → R$ 15 de taxa e R$ 85 para o montador).
- **Região:** definida pelo CEP no cadastro (ViaCEP). Montadores só veem pedidos da própria cidade e são avisados por push quando surge um novo.
- **Negar serviço:** o montador pode recusar um pedido do feed (some só para ele) ou desistir de uma proposta antes do pagamento (o cliente é avisado).
- **Comprovação:** o montador só conclui o serviço enviando uma foto tirada pela câmera.

## Segurança

- Login e senha ficam no **Firebase Authentication**. O app envia o ID token do Firebase em todas as chamadas e a API valida a assinatura com as chaves do Google; o usuário é sempre identificado pelo token, nunca por um ID enviado pelo app.
- Toda regra de negócio (quem pode aceitar, pagar, concluir, ver um pedido) é validada na API.
- As regras do Firestore estão em `MoblyUpApp/firestore.rules` e precisam ser publicadas no Console do Firebase (Firestore Database → Regras).

## Notificações push

A API envia notificações pelo Expo Push em cada etapa do pedido. No Android elas exigem um *development build* (o Expo Go não recebe push desde o SDK 53):

```bash
cd MoblyUpApp
npx eas-cli login
npx eas-cli init
npx eas-cli build --profile development --platform android
npx expo start --dev-client
```

Também é preciso configurar as credenciais do Firebase Cloud Messaging no EAS: <https://docs.expo.dev/push-notifications/fcm-credentials/>.

## Limitações conhecidas

- O pagamento é simulado (o cliente confirma o PIX no app); não há gateway de pagamento integrado.
- As fotos ficam salvas no disco da API (`wwwroot/uploads`); em produção o ideal é um armazenamento em nuvem.
- O botão "Central de Suporte" ainda não tem um canal de contato definido.
