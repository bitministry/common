

all the `System.SqlClient` objects are disposed within each method 

hook data to your T-SQL fluently 


`var resultsArray = "SELECT id, name FROM myTable".SqlRead(x => new { id = x.GetInt(0), name = x.GetString(1) });`

(SqlDataReader)


`var results = "SELECT id, name FROM myTable".SqlData(x => new { id = x["id"], name = x["name"] });`

(DataSet and SqlDataAdapter)


`"UPDATE myTable SET name=`andrew` WHERE id=1".SqlNonQuery();`

`var name = "SELECT name FROM myTable WHERE id=1".SqlScalar();`

`var result = "some SQL statament".SqlCommandFunk( ( SqlCommand com ) => {  ... do something with the com } );`

(SqlCommand)


---


> nuget <https://www.nuget.org/profiles/bitMinistry.com>

> open source at <https://github.com/bitministry/common>
