/*
This File contains different scripts to extract needed data based on the SSOP Database.
*/
------------------------------------------------------------
------------------------------------------------------------
-- Create New Table for Article Properties
-- Declare Stored Procedure
CREATE OR ALTER PROCEDURE ArticleCTE
				@ArticleId int
			AS
			BEGIN
				SET NOCOUNT ON;
				DROP TABLE IF EXISTS dbo.#Temp;
				DROP TABLE IF EXISTS dbo.#Union;
				WITH Parts(AssemblyID, ComponentID, PerAssemblyQty, ComponentLevel) AS  
				(  
					SELECT b.ArticleParentId, b.ArticleChildId, CAST(b.Quantity AS decimal),0 AS ComponentLevel  
					FROM dbo.M_ArticleBom  AS b  
					join dbo.M_Article a on a.Id = b.ArticleParentId
					where @ArticleId = a.Id
					UNION ALL  
					SELECT bom.ArticleParentId, bom.ArticleChildId, CAST(PerAssemblyQty * bom.Quantity as DECIMAL), ComponentLevel + 1  
					 FROM dbo.M_ArticleBom  AS bom  
						INNER join dbo.M_Article ac on ac.Id = bom.ArticleParentId
						INNER JOIN Parts AS p ON bom.ArticleParentId = p.ComponentID  
				)
				select * into #Temp 
				from (
					select pr.Id,pr.Name, Sum(p.PerAssemblyQty) as qty, pr.ToBuild as ToBuild
					FROM Parts AS p INNER JOIN M_Article AS pr ON p.ComponentID = pr.Id
					Group By pr.Id, pr.Name, p.ComponentID, pr.ToBuild) as x

				select * into #Union from (
					select Sum(o.Duration * t.qty) as dur, sum(t.qty) as count ,0 as 'Po'
						from dbo.M_Operation o join #Temp t on t.Id = o.ArticleId
						where o.ArticleId in (select t.Id from #Temp t)
				UNION ALL
					SELECT SUM(ot.Duration) as dur, COUNT(*) as count , 0 as 'Po'
						from dbo.M_Operation ot where ot.ArticleId = @ArticleId ) as x
				UNION ALL 
					SELECT 0 as dur, 0 as count, sum(t.qty) + 1 as 'Po'
					from #Temp t where t.ToBuild = 1
				select @ArticleId as ArticleId, Sum(u.dur) as SumDuration , sum(u.count) as SumOperations, sum(u.Po)  as ProductionOrders from #Union u
			END

-- Create new Table
CREATE TABLE ArticleStats (
	ArticleId int,
	SumDuration int,
	SumOperation int,
	ProductionOrders int
);

-- Declare Cursor
DECLARE ArticleCursor CURSOR
FOR Select Id from M_Article WHERE ArticleTypeId = '10007'

DECLARE @currArticleId INT;
OPEN ArticleCursor;

FETCH NEXT FROM ArticleCursor INTO @currArticleId;

WHILE @@FETCH_STATUS = 0
    BEGIN
        --PRINT @currArticleId;
		INSERT INTO ArticleStats EXEC ArticleCTE @currArticleId;
        FETCH NEXT FROM ArticleCursor INTO @currArticleId;
    END;

CLOSE ArticleCursor;

DEALLOCATE ArticleCursor;

-- Exec SP
DECLARE @ArticleId INT;
SET @ArticleId = 10033;
EXEC ArticleCTE @ArticleId
			
-- Map Article.Name to Order.Name with ArticleStats
SELECT 
Orders.CreationTime,
ArticleStats.SumDuration as SumDuration, 
ArticleStats.SumOperation as SumOperation, 
ArticleStats.ProductionOrders as ProductionOrders,
(Orders.ProductionFinishedTime - Orders.CreationTime) as CycleTime
FROM [TestResults].[dbo].[SimulationOrders] as Orders
INNER JOIN [Test].[dbo].[M_Article] as Article ON Orders.Name = Article.Name 
INNER JOIN [Test].[dbo].[ArticleStats] as ArticleStats ON Article.Id = ArticleStats.ArticleId
Where Orders.CreationTime >= 3360
ORDER BY Orders.CreationTime
------------------------------------------------------------
------------------------------------------------------------
-- Pivot KPIs for better extraction
use TestResults;
declare @simNr   NVARCHAR(MAX) = '',
		@from    NVARCHAR(MAX) = '',
		@to      NVARCHAR(MAX) = '',
		@columns NVARCHAR(MAX) = '', 
		@sql     NVARCHAR(MAX) = '';
​
set @simNr = 1001
set	@from = 3360
set	@to = 80180
​
-- get all categories
SELECT @columns+=QUOTENAME("Name") + ','
FROM (Select distinct name from Kpis where SimulationNumber = @simNr) as n
​
-- remove the last comma
SET @columns = LEFT(@columns, LEN(@columns) - 1);
​
-- dynamic load
Set @sql = '
select Time, Lateness, TotalSetup, InDue, OverDue, New, Product, AdherenceToDue, TotalWork, Finished, OEE, Assembly, OverDueTotal, CycleTime, Tardiness, Material, IndueTotal, "Open" from (
	select 
		Name,
		Value,
		Time
	from Kpis k
	where
		SimulationNumber = ' + @simNr + ' and Time between ' + @from + ' and ' + @to +'
		) t
		Pivot(
			Sum(Value)
			FOR "Name" IN (
				' + @columns + '
			)
		) as pivot_table
		order by Time;';
Execute sp_executesql @sql;

------------------------------------------------------------
------------------------------------------------------------
-- Get Total Workload of Resources
-- Scroll trough all view high workload

SELECT Time, Name, SUM(Value) as 'Total Workload' 
  FROM Kpis
  WHERE SimulationNumber = '30241' and KpiType in (1,2) and Time > 3360
  Group by Name, Time
  
  
---- Get Workload of Capabilities without null values
DECLARE @SIMNUM AS INT
SET @SIMNUM = 10001
DROP TABLE IF EXISTS #Temp
SELECT *
INTO #Temp
FROM
(SELECT Name, Sum(Value) as value, time
FROM Kpis
where SimulationNumber = @SIMNUM and KpiType in (1,2) and Value != 0
group by name, time) AS x
SELECT SUBSTRING(Name,0,20) as Capability, AVG(Value) as TotalWorkload
FROM #Temp  
--WHERE Name Like 'Res%'
GROUP BY SUBSTRING(Name,0,20)--, KpiType
order by TotalWorkload desc
