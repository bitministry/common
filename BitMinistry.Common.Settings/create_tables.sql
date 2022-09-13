create schema bm ;
 go

CREATE TABLE bm.Setting(
	Name varchar(50) NOT NULL,
	NumericValue numeric(14, 2) NULL,
	NTextValue ntext NULL,
	DateTimeValue datetime NULL,
	Comment varchar(111) NULL
) ;
 GO 


CREATE TABLE bm.Source(
	Type varchar(22) NOT NULL,
	Title varchar(33) NOT NULL,
	Url varchar(333) NOT NULL,
	Target varchar(50) NOT NULL,
	Ranking int NOT NULL,
	Lang char(2) NULL
) ;
 GO

