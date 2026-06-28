# Controle Financeiro

## Objetivo
Aplicativo desktop offline-first projetado para consolidacao financeira pessoal por meio de importacao manual de extratos. Permite classificar transacoes, acompanhar metas orcamentarias mensais e registrar a evolucao de investimentos com foco em privacidade local.

## Requisitos e Configuracao
1. Instale o .NET 9 SDK no sistema operacional Windows.
2. Clone este repositorio.
3. Restaure as dependencias executando:
   ```bash
   dotnet restore
   ```
4. O banco de dados SQLite local (`finance.db`) e criado e migrado de forma automatica na primeira inicializacao da aplicacao.

## Como Usar
1. **Compilar e Executar:**
   Execute o projeto principal:
   ```bash
   dotnet run --project ControleFinanceiroForms/ControleFinanceiroForms.csproj
   ```
2. **Definir Orcamento:**
   Na aba "Categorias", adicione suas categorias de gastos e defina um limite orcamentario mensal para cada uma.
3. **Importar Extratos:**
   Na aba "Importar Transacoes", selecione um arquivo de extrato nos formatos CSV ou PDF. Defina a categoria padrao se desejar e execute a importacao. Lançamentos duplicados sao ignorados usando hash criptografico.
4. **Visualizar Status:**
   A aba "Dashboard" consolida automaticamente os gastos do mes corrente contra os limites definidos, sinalizando visualmente em caso de excesso de gastos.
5. **Investimentos:**
   A aba "Meus Investimentos" permite registrar periodicamente o saldo total de seu patrimonio para acompanhamento de evolucao.
