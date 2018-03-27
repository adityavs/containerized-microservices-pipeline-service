#Hack. This sleep statement is required 
sleep 1s

#start the script to create the DB and data AND THEN start the sqlserver
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Password1! -i createdb.sql & /opt/mssql/bin/sqlservr