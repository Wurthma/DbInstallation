
# DbInstallation

> Console Application desenvolvido com ASP.NET Core 3.1 para
> automatização de execução de scripts SQL para instalação e atualização
> de  banco de dados Sql Server e Oracle.

## Fluxo da aplicação

 1. Deve ser informado o tipo de banco de dados (Sql Server ou Oracle)
    que deseja se conectar.
 2. Serão solicitados os dados para conexão com o banco de dados e serão realizados testes básicos de conexão.
 3. Deve ser informado se deseja fazer a **instalação** ou **atualização** da base de dados.
 4. Após selecionar a operação serão realizadas as validações básicas antes de seguir com o processo.
 5. O processo e o log de execução poderá ser acompanhado pelo console onde o mesmo informará o arquivo em execução.

## Scripts SQL
Todos os scripts a serem executados ficam em **\Database\SqlScript** e nesse diretório é feita a divisão entre Oracle (diretório **\Oracle**). e SQL Server (diretório **\SqlServer**).

Tanto no diretório do Oracle quanto no do SQL Server serão encontrados os diretórios **"\Install"** e **"\Update"**, sendo que o update deve conter os diretórios de cada versão (numéricos).

No código da aplicação, a classe **FileHelper** possui dicionários com a estrutura de pastas de cada tipo de banco de dados (Oracle e Sql Server) com a ordem em que devem ser executados lidos e executados. Altere esses dicionários (OracleListFolder e SqlServerListFolder) se desejar alterar a estrutura de diretórios.

**OracleListFolder:** 

    private static Dictionary<int, string> OracleListFolder { 
    	get 
    	{
    		return new Dictionary<int, string>
    		{
    			{ 0, @"\Platypus" },
    			{ 1, @"\Table" },
    			{ 2, @"\Constraints" },
    			{ 3, @"\Sequence" },
    			{ 4, @"\Functions" },
    			{ 5, @"\Procedure" },
    			{ 6, @"\Package" },
    			{ 7, @"\Trigger" },
    			{ 8, @"\View" },
    			{ 9, @"\Carga" }
    		};
    	} 
    }

**SqlServerListFolder:** 

    private static Dictionary<int, string> SqlServerListFolder { 
    	get 
    	{
    		return new Dictionary<int, string>
    		{
    			{ 0, @"\Platypus" },
    			{ 1, @"\Table" },
    			{ 2, @"\Constraints" },
    			{ 3, @"\Functions" },
    			{ 4, @"\Procedure" },
    			{ 5, @"\Trigger" },
    			{ 6, @"\View" },
    			{ 7, @"\Carga" }
    		};
    	} 
    }

Não é possível criar subdiretórios nessa estrutura, se criado, arquivos dentro destes não serão lidos.

Os arquivos **.sql* serão executados por ordem alfabética, para manter a ordem é sugerido nomear os arquivos com prefixo numérico. Exemplo: 

 - "01_CARGA_ADM.sql" 
 - "02_CARGA.sql" 
 - "03_CARGA_2.sql"

**Atenção:** scripts Oracle são capturados com Regex, pois seu driver não permite a execução de vários comandos ao mesmo tempo como feito no Sql Server. Para comandos **DML** seria possível utilizando um bloco Pl/Sql **"Begin {comandos DML} End;"** mas qualquer comando **DDL** que fosse colocado junto ao script comprometeria o funcionamento.

## Instruções de formatação de scripts SQL

**Oracle:**

 1. Sempre separar os comando por uma nova linha terminando todos eles
    com **";"**. Exemplo:

    INSERT INTO MYTABLE (COLUMN1,COLUMN2, COLUMN3) VALUES (1, "Lorem Ipsum", 0);
    INSERT INTO MYTABLE (COLUMN1,COLUMN2, COLUMN3) VALUES (2, "Dolor sit amet", 1);

 2. Pode ser necessário utilizar durante a execução dos comando Oracle o
    **Owner** ou **Tablespaces**. Esses dados são solicitados se a opção Oracle for escolhida e serão substituídos nos arquivos onde for encontrado **&OWN**,
    **&TD** e **&TI**, sendo:

	 - &OWN = Owner do BD.
	 - &TD = Tablespace de dados
	 - &TI = Tablespace  de index.

 3. Blocos Pl/Sql não devem ser misturado com comandos DDL ou DML fora
    do bloco.
 4. Arquivos nos diretórios Platypus, Function, Procedures, Package, Trigger e View devem separar cada bloco Pl/Sql com o caracter **"/"** do Oracle. Dessa forma podem ser escritos vários blocos Pl/Sql em um mesmo arquivo **.sql** (se há um único bloco o final dele também deve utilizar o caracter).
 5. No **appsetting.json** no item **Settings** é possível configurar uma forma explicita de indicar que um arquivo utiliza blocos Pl/Sql para que o mesmo utilize o Regex corretamente. Utilize a configuração **ExplicitSetPlSqlCommand** e todo arquivo que conter o texto definido utilizará o Regex para capturar blocos Pl/Sql.
	 - **Atenção:** se deseja utilizar blocos Pl/Sql em qualquer diretório que não seja algum do item #4, será necessário o uso dessa configuração.
 6. **Não utilizar nenhum comando do Sqlplus**, o driver do Oracle não reconhece esses comandos.

**SQL Server:**

 1. Não deve ser utilizado o "GO" do Sql Server nos seus scripts, pois  o driver não reconhece o comando.

*No Sql Server há uma liberdade maior para os scripts já que o mesmo não precisa de  Regex, pois seu driver não limita o uso de comando DDL e DML como ocorre no Oracle.*

## Configurações gerais

1. No **appsetting.json** no item **Settings** é possível colocar a descrição do produto/projeto (ProjectDescription). Essa descrição será utilizada para gravar a versão atual do produto no banco de dados.

2. No diretório **"\Update"** todos os diretórios devem ser numéricos e sequenciais. O nome do  diretório representa o número da versão para qual está sendo atualizada e dentro de cada pasta deve conter os scripts com as alterações daquela versão utilizando a mesma estrutura de pasta mostradas anteriormente nos dicionários.

3. Atualmente a tabela que grava a versão do bando de dados não é parametrizada.

## Argumentos

**-newupdt**: cria a próxima pasta de update para Oracle e Sql Server. Se a ultima pasta  é a "95" automaticamente será criada toda estrutura da "96".

**-oracle <owner> <password> <tnsname> <tablespaceData> <tablespaceIndex> <i | u | gint>**: argumentos usados para instalação, atualização ou geração da validação de integridade do oracle ("i" para instalação, "u" para atualização ou gint para "generate integrity validation").

**-sqlserver [<user>] [<password>] <server> <databaseName> <i | u>**: argumentos para instalação ou atualização do Sql Server ("i" para instalação e "u" para atualização).

**-sqlserver <server> <databaseName> <i | u>**: argumentos para instalação ou atualização do Sql Server com **trusted connection** ("i" para instalação e "u" para atualização).