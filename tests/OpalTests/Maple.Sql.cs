// Copyright 2013-2019 Mark Fichtner
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Maple.Sql
{
    internal static class AttribExt
    {
        /// <summary>
        /// Returns a custom attribute from a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this Type type, bool inherited = false) where T:Attribute
        {
            T result;
            var attrs = type.GetCustomAttributes(typeof(T), inherited);
            if (attrs.Length > 0)
                result = attrs[0] as T;
            else
                result = null;
            return result;
        }

        /// <summary>
        /// Returns custom attribute for a property / method / field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this MemberInfo type, bool inherited = false) where T : Attribute
        {
            T result;
            var attrs = type.GetCustomAttributes(typeof(T), inherited);
            if (attrs.Length > 0)
                result = attrs[0] as T;
            else
                result = null;
            return result;
        }

        /// <summary>
        /// Returns custom attribute for a property / method / field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="inherited"></param>
		/// <param name="attr">Returned attribute</param>
        /// <returns>True if attribute found</returns>
        public static bool TryGetAttribute<T>(this MemberInfo type, bool inherited, out T attr) where T : Attribute
        {
            var attrs = type.GetCustomAttributes(typeof(T), inherited);
            bool isOk = attrs.Length > 0;
            attr = isOk ? attrs[0] as T : null;
            return isOk;
        }
    }
    
    /// <summary>
    /// Provides additional details about a field mapping to a column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute: Attribute
    {
		/// <summary>
		/// Default constructor
		/// </summary>
		public ColumnAttribute()
        {
        }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="columnName">Column name</param>
        public ColumnAttribute(string columnName)
        {
            _columnName = columnName;
        }

        #region Properties

        #region ColumnName property
        /// <summary>
        /// Column name in database
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }
        private string _columnName;
        #endregion

        #region IsPrimaryKey
        /// <summary>
        /// True if column is primary key
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        #endregion

        #endregion
    }

    /// <summary>
    /// Database exception
    /// </summary>
    [Serializable]
    public class DatabaseException: Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseException()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseException(Exception inner, string msg)
            : base(msg, inner)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseException(Exception inner, string fmt, params object[] args)
            : base(string.Format(fmt, args), inner)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseException(string fmt, params object[] args)
            : base(string.Format(fmt, args))
        {
        }

        /// <summary>
        /// Creates more specific exception based on info in SqlException
        /// </summary>
        /// <param name="inner">SqlException</param>
        /// <returns>Specific DatabaseException</returns>
        public static DatabaseException Create(SqlException inner)
        {
            DatabaseException ex;
            switch (inner.Number)
            {
                case 53:
                    ex = new NetworkException(inner, inner.Message);
                    break;
                case 11001:
                    ex = new NetworkException(inner, "Connection network error");
                    break;
                case 10061:
                    ex = new NetworkException(inner, "Local network error");
                    break;
                case 2812:
                    ex = new MissingProcedureException(inner);
                    break;
                case 18487:
                case 18488:
                    ex = new ConnectionException(inner);
                    break;
                default:
                    ex = new DatabaseException(inner, "Unspecified database error");
                    break;
            }
            throw ex;
        }


        /// <summary>
        /// Throws specific DatabaseException based on info in SqlException
        /// </summary>
        /// <param name="inner">SqlException</param>
        public static void Throw(SqlException inner)
        {
            throw Create(inner);
        }
    }

    /// <summary>
    /// Exception throw when the server is not reachable
    /// </summary>
    [Serializable]
    public class NetworkException: DatabaseException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NetworkException(Exception inner, string msg)
            : base(inner, msg)
        {
        }
    }

    /// <summary>
    /// Exception thrown when stored procedure doesn't exist
    /// </summary>
    [Serializable]
    public class MissingProcedureException: DatabaseException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MissingProcedureException(Exception inner)
            : base(inner, inner.Message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when unable to openConn database connection
    /// </summary>
    [Serializable]
    public class ConnectionException: DatabaseException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectionException(Exception inner)
            : base(inner, inner.Message)
        {
        }
    }
    
    /// <summary>
    /// An indexed object that initially holds an index, will resolve
    /// to the full IndexObject using the Getter method
    /// </summary>
    public class DelayedObject<T> where T: IndexedObject
    {
        private T _object;
        private int _id;
        private Func<int, T> _getter;

        /// <summary>
        /// Creates a delayed object with index 'id' and getter function
        /// for resolving id when the full object is needed
        /// </summary>
        public DelayedObject(int id, Func<int, T> getter)
        {
            _id = id;
            _getter = getter;
        }

        /// <summary>
        /// Constructs a delayed object with the full object
        /// </summary>
        /// <param name="cpy"></param>
        public DelayedObject(T cpy)
        {
            _object = cpy;
            _id = cpy.Id;
        }

        /// <summary>
        /// Id of object
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Returns full object
        /// </summary>
        static public implicit operator T(DelayedObject<T> obj)
        {
            if (obj._object == null)
                obj._object = obj._getter(obj._id);

            return obj._object;
        }

        
        /// <summary>
        /// Returns true if the object has been resolved
        /// </summary>
        static public implicit operator bool(DelayedObject<T> obj)
        {
            return obj._object != null;
        }

        /// <summary>
        /// Casts an indexed object into a delayed object
        /// </summary>
        static public implicit operator DelayedObject<T>(T cpy)
        {
            return new DelayedObject<T>(cpy);
        }
    }

    /// <summary>
    /// Builds expressions for expressions in this module
    /// </summary>
    public static class ExprBuilder
    {
        static Dictionary<Type, MethodInfo> _typeToNextMap;

        static ExprBuilder()
        {
            var readerType = typeof(Reader);

            var nextNullable = readerType.GetMethod("NextNullable");

            _typeToNextMap = new Dictionary<Type, MethodInfo>()
            {
                {   typeof(bool),   readerType.GetMethod("NextBoolean") },
                {   typeof(bool?),  nextNullable.MakeGenericMethod(typeof(bool)) },
                {   typeof(int),    readerType.GetMethod("NextInt") },
                {   typeof(int?),  nextNullable.MakeGenericMethod(typeof(int)) },
                {   typeof(short),  readerType.GetMethod("NextInt16") },
                {   typeof(short?),  nextNullable.MakeGenericMethod(typeof(short)) },
                {   typeof(long),   readerType.GetMethod("NextInt64") },
                {   typeof(long?),  nextNullable.MakeGenericMethod(typeof(bool)) },
                {   typeof(float),  readerType.GetMethod("NextFloat") },
                {   typeof(float?),  nextNullable.MakeGenericMethod(typeof(float)) },
                {   typeof(double), readerType.GetMethod("NextDouble") },
                {   typeof(double?),  nextNullable.MakeGenericMethod(typeof(double)) },
                {   typeof(DateTime), readerType.GetMethod("NextDateTime") },
                {   typeof(DateTime?),  nextNullable.MakeGenericMethod(typeof(DateTime)) },
                {   typeof(Guid),   readerType.GetMethod("NextGuid") },
                {   typeof(Guid?),  nextNullable.MakeGenericMethod(typeof(Guid)) },
                {   typeof(string), readerType.GetMethod("NextString") },
                {   typeof(byte[]), readerType.GetMethod("NextBlob") },
            };
        }

        /// <summary>
        /// Returns Reader.NextXXXX method expression call
        /// </summary>
        /// <param name="reader">Reader expression</param>
        /// <param name="type">Column type</param>
        /// <returns>Call expression</returns>
        public static MethodCallExpression GetReaderNext(Expression reader, Type type)
        {
            MethodCallExpression result;
            MethodInfo methodInfo;
            if (_typeToNextMap.TryGetValue(type, out methodInfo))
                result = Expression.Call(reader, methodInfo);
            else
                result = null;
            return result;
        }


        /// <summary>
        /// Creates a SQL command parameter expression
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns>Expression for creating a SQL parameter</returns>
        public static Expression ToParameterExpr(string name, Expression value)
        {
            var nameConst = Expression.Constant(name);
            var nullConst = Expression.Constant(DBNull.Value);
            Expression result;

            var sqlParameterType = typeof(SqlParameter);
            var ctor = sqlParameterType.GetConstructor(new Type[] { typeof(string), typeof(object) });

            var type = value.Type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var param = Expression.Parameter(value.Type);
                var label = Expression.Label(sqlParameterType);

                // new SqlParameter(name, value.HasValue ? value.Value : DBNull.Value)

                result = Expression.Block(sqlParameterType,
                    new ParameterExpression[] { param },
                    Expression.Assign(param, value),
                    Expression.Return(label,
                        Expression.New(ctor,
                            nameConst,
                            Expression.Condition(
                                Expression.Property(param, "HasValue"),
                                Expression.Convert(Expression.Property(param, "Value"), typeof(object)),
                                Expression.Convert(nullConst, typeof(object))))),
                    Expression.Label(label, Expression.Constant(null, sqlParameterType)));
            }
            else if (type.IsValueType)
            {
                if (type == typeof(uint))
                    value = Expression.Convert(value, typeof(int));
                else if (type == typeof(ulong))
                    value = Expression.Convert(value, typeof(long));

                result = Expression.New(ctor,
                    nameConst,
                    Expression.Convert(value, typeof(object)));
            }
            else if (type == typeof(System.Xml.XmlReader))
            {
                var param = Expression.Parameter(value.Type);
                var label = Expression.Label();
                result = Expression.Block(sqlParameterType,
                    new ParameterExpression[] { param },
                    Expression.Assign(param, value),
                    Expression.Return(label,
                        Expression.Condition(
                            Expression.Equal(value, Expression.Constant(null, type)),
                            Expression.New(ctor, nameConst, nullConst),
                            Expression.MemberInit(
                                Expression.New(ctor, nameConst, Expression.Constant(SqlDbType.Xml)),
                                Expression.Bind(sqlParameterType.GetProperty("Value"), param)))),
                    Expression.Label(label));
            }
            else
            {
                var objP = Expression.Convert(
                        Expression.Coalesce(Expression.Convert(value, typeof(object)), nullConst),
                        typeof(object));

                result = Expression.New(ctor, nameConst, objP);
            }
            return result;
        }

    }
    
    /// <summary>
    /// When specified, instructs system to ignore property when creating an entity method
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited=true)]
    public class IgnoreAttribute: Attribute
    {
    }
    
    /// <summary>
    /// Database mapped object containing an id
    /// </summary>
    public class IndexedObject
    {
        /// <summary>
        /// Sets id of this object to next int in stream
        /// </summary>
        public IndexedObject(Reader reader)
        {
            _id = reader.NextInt();
        }

        /// <summary>
        /// Sets id of this object to first column of reader
        /// </summary>
        /// <param name="reader"></param>
        public IndexedObject(IDataReader reader)
        {
            _id = reader.GetInt32(0);
        }
        
        /// <summary>
        /// Sets id of this object to first column of row
        /// </summary>
        public IndexedObject(DataRow row)
        {
            _id = (int)row[0];
        }

        /// <summary>
        /// Sets Id to 'id'
        /// </summary>
        /// <param name="id"></param>
        protected IndexedObject(int id)
        {
            _id = id;
        }

        #region Id Property
        /// <summary>
        /// Id (primary key) of object
        /// </summary>
        public int Id 
        { 
            get { return _id; } 
        }
        private int _id;

        #endregion

        /// <summary>
        /// Converts a sequence of indexed objects into an xml reader with the format
        /// <r s="id" />
        /// </summary>
        static public XmlReader ToXml(IEnumerable<IndexedObject> objs)
        {
            var memoryStream = new MemoryStream();
            var writer = XmlWriter.Create(memoryStream);
            writer.WriteStartElement("r");
            foreach (IndexedObject obj in objs)
            {
                writer.WriteStartElement("s");
                writer.WriteValue(obj.Id);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Close();
            memoryStream.Position = 0;
            return XmlReader.Create(memoryStream);
        }

        /// <summary>
        /// Returns true obj's type and id match this object
        /// </summary>
        public override bool Equals(object obj)
        {
            bool result = false;
            if ((obj != null) && (obj.GetType() == GetType()))
                result = (obj as IndexedObject).Id == Id;
            return result;
        }

        /// <summary>
        /// Returns true if the objects' types and ids match
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(IndexedObject left, IndexedObject right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return true;
            else if (((object) left == null) || ((object) right == null))
                return false;
            else if (left.GetType() != right.GetType())
                return false;
            else 
                return (left.Id == right.Id);
        }

        /// <summary>
        /// Returns true if the objects' type or id don't match
        /// </summary>
        public static bool operator !=(IndexedObject left, IndexedObject right)
        {
            if (System.Object.ReferenceEquals(left, right))
                return false;
            else if (((object)left == null) || ((object)right == null))
                return true;
            else if (left.GetType() != right.GetType())
                return true;
            else if (right == null)
                return true;
            else 
                return (left.Id != right.Id);
        }

        /// <summary>
        /// Returns the hash code of the id
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns id 
        /// </summary>
        public override string ToString()
        {
            return Id.ToString();
        }
    }

    /// <summary>
    /// Information Schema
    /// </summary>
    public class InformationSchema
    {
        private readonly SqlDb _db;

        /// <summary>
        /// Constructor
        /// </summary>
        public InformationSchema(SqlDb db)
        {
            _db = db;
        }

        #region Properties

        #region Tables Property

        /// <summary>
        /// INFORMATION_SCHEMA.TABLES
        /// </summary>
        public InformationSchemaTables Tables
        {
            get
            {
                if (_tables == null)
                    _tables = new InformationSchemaTables(_db);
                return _tables;
            }
        }
        private InformationSchemaTables _tables;
        #endregion

        #region Columns Property
        /// <summary>
        /// Returns view to column information
        /// </summary>
        public InformationSchemaColumns Columns
        {
            get
            {
                if (_columns == null)
                    _columns = new InformationSchemaColumns(_db);
                return _columns;
            }
        }
        private InformationSchemaColumns _columns;
        #endregion

        #endregion
    }

    /// <summary>
    /// INFORMATION_SCHEMA.COLUMN row
    /// </summary>
    public class InformationSchemaColumn
    {
        internal InformationSchemaColumn(string tableName, string columnName, int ordinal)
        {
			_tableName = tableName;
			_columnName = columnName;
			_ordinal = ordinal;
        }

		#region Properties

		#region TableName Property
		/// <summary>
		/// Returns name of owning table
		/// </summary>
		public string TableName
		{
			get { return _tableName; }
		}
		private readonly string _tableName;
		#endregion

		#region ColumnName Property

		/// <summary>
		/// Column name
		/// </summary>
		public string ColumnName
		{
			get { return _columnName; }
		}
		private readonly string _columnName;
		#endregion

		#region Ordinal Property
		/// <summary>
		/// Column identification number
		/// </summary>
		public int Ordinal
		{
			get { return _ordinal; }
		}
		private readonly int _ordinal;
		#endregion

		/// <summary>
		/// Column data type
		/// </summary>
		public string DataType { get; internal set; }


        /// <summary>
        /// Returns true if column can contain nulls
        /// </summary>
        public bool IsNullable { get; internal set; }

        #endregion
    }

    /// <summary>
    /// INFORMATION_SCHEMA.COLUMNS
    /// </summary>
    public class InformationSchemaColumns: IEnumerable<InformationSchemaColumn>
    {
        private readonly SqlDb _db;

        internal InformationSchemaColumns(SqlDb db)
        {
            _db = db;
        }


        #region Properties
        /// <summary>
        /// Returns name of schema view for getting database column info
        /// </summary>
        public string TableName
        {
            get { return "INFORMATION_SCHEMA.COLUMNS"; }
        }
        #endregion


        /// <summary>
        /// Returns columns names of belonging table
        /// </summary>
        /// <param name="tableName">Table name</param>
        public IEnumerable<string> GetColumnsNames(string tableName)
        {
            return _db.Query(
                rdr => rdr.NextString(),
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@0",tableName);
        }

		/// <summary>
		/// Returns all columns
		/// </summary>
		public IEnumerable<InformationSchemaColumn> Find(string tableName)
		{
			return _db.Query(
				rdr => new InformationSchemaColumn(
					tableName: rdr.NextString(),
					columnName: rdr.NextString(),
					ordinal: rdr.NextInt()
					)
				{
					DataType = rdr.NextString(),
					IsNullable = rdr.NextString().ToLower() == "YES"
				},
@"SELECT TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION,DATA_TYPE,IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME=@0",
					tableName
				);
		}


		/// <summary>
		/// Returns all columns
		/// </summary>
		public IEnumerator<InformationSchemaColumn> GetEnumerator()
        {
            return _db.Query(
                rdr => new InformationSchemaColumn(
					tableName:rdr.NextString(),
					columnName:rdr.NextString(),
					ordinal:rdr.NextInt()
					)
                {
                    DataType = rdr.NextString(),
                    IsNullable = rdr.NextString().ToLower() == "YES"
                },
@"SELECT TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION,DATA_TYPE,IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS"
                ).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
    
    /// <summary>
    /// INFORMATION_SCHEMA.TABLES row
    /// </summary>
    public class InformationSchemaTable
    {
        internal InformationSchemaTable(string catalog, string schema, string name)
        {
			_catalog = catalog;
			_schema = schema;
			_name = name;
        }

		#region Catalog Property
		/// <summary>
		/// Table catelog
		/// </summary>
		public string Catalog
		{
			get { return _catalog; }
		}
		private readonly string _catalog;
		#endregion

		#region Schema Property
		/// <summary>
		/// Table schema
		/// </summary>
		public string Schema
		{
			get { return _schema; }
		}
		private readonly string _schema;
		#endregion

		#region Name Property
		/// <summary>
		/// Table name
		/// </summary>
		public string Name
		{
			get { return _name; }
		}
		private readonly string _name;
		#endregion
	}

    /// <summary>
    /// INFORMATION_SCHEMA.TABLES
    /// </summary>
    public class InformationSchemaTables: IEnumerable<InformationSchemaTable>
    {
        private readonly SqlDb _db;
        
        internal InformationSchemaTables(SqlDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns all table names
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                return _db.Query(
                    rdr => rdr.NextString(),
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");
            }
        }


        /// <summary>
        /// Returns all tables
        /// </summary>
        public IEnumerator<InformationSchemaTable> GetEnumerator()
        {
            return _db.Query(
                rdr => new InformationSchemaTable(
					catalog: rdr.NextString(),
					schema: rdr.NextString(),
					name: rdr.NextString()
					),
                "SELECT TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME FROM INFORMATION_SCHEMA.TABLES")
                .GetEnumerator();
        }

		/// <summary>
		/// Returns table named 'tableName'
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public InformationSchemaTable Get(string tableName)
		{
			return _db.QueryFirst(
				rdr => new InformationSchemaTable(
					catalog: rdr.NextString(),
					schema: rdr.NextString(),
					name: tableName
				),
@"SELECT TABLE_CATALOG,TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME=@0", 
					tableName
				);
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// Encapsulates an IDataReader object, ensuring the connection is closed when
    /// the reader is closed
    /// </summary>
    public class Reader: IDataReader, IDisposable
    {
        private bool _isDisposed;
        private readonly SqlCommand _command;
        private readonly SqlDataReader _dataReader;
        private bool _next;
		private bool _hasOpened;

        /// <summary>
        /// openConn connection and executes command.  
        /// </summary>
        /// <remarks>
        /// The Reader takes ownership of the command and closes / disposes it
        /// in the closeConn / Dispose method.
        /// </remarks>
        public Reader(SqlCommand command)
        {
            _command = command;
            if (_command.Transaction == null)
            {
                if (_command.Connection.State == ConnectionState.Closed)
				{
                    try
                    {
                        _command.Connection.Open();
                        _hasOpened = true;
                    }
                    catch (SqlException ex)
                    {
                        throw DatabaseException.Create(ex);
                    }
                }

				try
                {
                    _dataReader = _command.ExecuteReader();
                }
                catch
                {
					if (_hasOpened)
					{
						_command.Connection.Close();
						_hasOpened = false;
					}
                    throw;
                }
            }
            else
            {
                _dataReader = _command.ExecuteReader();
            }
            _columnIndex = 0;
        }

		internal Reader(SqlCommand command, SqlDataReader reader, bool hasOpened)
		{
			_command = command;
			_dataReader = reader;
			_columnIndex = 0;
			_hasOpened = hasOpened;
		}

		
		/// <summary>
		/// Asynchronously opens, executes, and returns a wrapped reader
		/// </summary>
		/// <param name="command">Prep'd command</param>
		/// <returns>Task completing above</returns>
		public static async Task<Reader> CreateAsync(SqlCommand command)
		{
			var hasOpened = false;
			if (command.Connection.State == ConnectionState.Closed)
			{
                try
                {
                    await command.Connection.OpenAsync().ConfigureAwait(false);
                    hasOpened = true;
                }
                catch (SqlException ex)
                {
                    throw DatabaseException.Create(ex);
                }
            }
			
			try
			{
				var task = await command.ExecuteReaderAsync().ConfigureAwait(false);
				return new Reader(command, task, hasOpened);
			}
			catch
			{
				if (hasOpened)
					command.Connection.Close();
				throw;
			}
		}


		/// <summary>
		/// Closes the connection and disposes of the rdr / command
		/// </summary>
		public void Dispose()
		{
			if (!_isDisposed)
			{
				_dataReader.Dispose();
				if (_hasOpened)
				{
					_command.Connection.Close();
					_hasOpened = false;
				}
				_command.Dispose();
				_isDisposed = true;
			}
		}


		#region Properties

		#region ColumnIndex Property
		/// <summary>
		/// Returns the current column index
		/// </summary>
		public int ColumnIndex
        {
            get { return _columnIndex; }
        }
        private int _columnIndex;
        #endregion

        #region Depth Property
        /// <summary>
        /// Gets a value that indicates the depth of nesting for the current row.
        /// </summary>
        public int Depth
        {
            get { return _dataReader.Depth; }
        }
        #endregion

        #region FieldCount Property
        /// <summary>
        /// Gets the number of columns in the current row
        /// </summary>
        public int FieldCount
        {
            get { return _dataReader.FieldCount; }
        }
        #endregion

        #region IsClosed Property
        /// <summary>
        /// Returns true if the reader is closed
        /// </summary>
        public bool IsClosed
        {
            get { return _isDisposed; }
        }
        #endregion

        #region RecordsAffected Property
        /// <summary>
        /// Number of rows affected by transaction
        /// </summary>
        public int RecordsAffected
        {
            get { return _dataReader.RecordsAffected; }
        }
        #endregion

        #region Indexer[string] Property

        /// <summary>
        /// Returns value at column 'name'
        /// </summary>
        public object this[string name]
        {
            get { return _dataReader[name]; }
        }

        #endregion

        #region Indexer[int] Property

        /// <summary>
        /// Returns column value at ordinal 'i'
        /// </summary>
        public object this[int i]
        {
            get { return _dataReader[i]; }
        }

        #endregion

        #endregion


        /// <summary>
        /// Returns a System.Data.DataTable that describes the column metadata of the
        /// System.Data.SqlClient.SqlDataReader.
        /// </summary>
        public DataTable GetSchemaTable()
        {
            return _dataReader.GetSchemaTable();
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of
        /// batch Transact-SQL statements.
        /// </summary>
        public bool NextResult()
        {
            bool isOk;
            if (_next)
            {
                isOk = true;
                _next = false;
            }
            else
            {
                isOk = _dataReader.NextResult();
            }
            return isOk;
        }

		/// <summary>
		/// Advances the data reader to the next result, asynchronously
		/// </summary>
		public async Task<bool> NextResultAsync()
		{
			bool isOk;
			if (_next)
			{
				isOk = true;
				_next = false;
			}
			else
			{
				isOk = await _dataReader.NextResultAsync().ConfigureAwait(false);
			}
			return isOk;
		}


		/// <summary>
		/// Advances the reader to the next record
		/// </summary>
		/// <returns>true if there are more rows; otherwise, false</returns>
		public bool Read()
        {
            var isOk = _dataReader.Read();
            if (isOk)
            {
                _columnIndex = 0;
            }
            else
            {
                if (NextResult())
                {
                    _next = true;
                }
                else
                {
                    Dispose();
                }
            }
            return isOk;
        }

		
		/// <summary>
		/// Advances the reader to the next record asynchronously
		/// </summary>
		/// <returns>true if there are more rows; otherwise, false</returns>
		public async Task<bool> ReadAsync()
		{
			var isOk = await _dataReader.ReadAsync().ConfigureAwait(false);
			if (isOk)
			{
				_columnIndex = 0;
			}
			else
			{
				if (await NextResultAsync().ConfigureAwait(false))
				{
					_next = true;
				}
				else
				{
					Dispose();
				}
			}
			return isOk;
		}


		/// <summary>
		/// Returns column as type T
		/// </summary>
		/// <typeparam name="T">Type to coerce column</typeparam>
		/// <param name="i">Zero-based column ordinal</param>
		/// <returns>Column as T</returns>
		public T Get<T>(int i)
        {
            return (T) _dataReader.GetValue(i);
        }

        
        /// <summary>
        /// Returns column as type T
        /// </summary>
        /// <typeparam name="T">Type to coerce column</typeparam>
        /// <param name="columnName">Column name</param>
        /// <returns>Column as T</returns>
        public T Get<T>(string columnName)
        {
            int i = _dataReader.GetOrdinal(columnName);
            if (i == -1)
                throw new ArgumentException(string.Format("Column '{0}' does not exist", columnName), "columnName");
            return (T)_dataReader.GetValue(i);
        }


        /// <summary>
        /// Returns value as T, or default value if column is null
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="i">Zero-based column ordinal</param>
        /// <returns>Column value or default if null</returns>
        public T GetSafe<T>(int i)
        {
            var o = _dataReader.GetValue(i);
            return (o is T) ? (T)o : default(T);
        }

        /// <summary>
        /// Returns value as T, or default value if column is null
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="columnName">Column name</param>
        /// <returns>Column value or default if null</returns>
        public T GetSafe<T>(string columnName)
        {
            int i = _dataReader.GetOrdinal(columnName);
            if (i == -1)
                throw new ArgumentException(string.Format("Column '{0}' does not exist", columnName), "columnName");
            var o = _dataReader.GetValue(i);
            return (o is T) ? (T)o : default(T);
        }


        /// <summary>
        /// Returns column as type T, or default value if column is null or doesn't match type
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="i">Zero-based index of ordinal</param>
        /// <param name="defaultValue"></param>
        /// <returns>Column as T, default value if null</returns>
        public T Get<T>(int i, T defaultValue)
        {
            var o = _dataReader.GetValue(i);
            return (o is T) ? (T)o : defaultValue;
        }

        /// <summary>
        /// Returns column as type T, or default value if column is null or doesn't match type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T Get<T>(string columnName, T defaultValue)
        {
            int i = _dataReader.GetOrdinal(columnName);
            if (i == -1)
                throw new ArgumentException(string.Format("Column '{0}' does not exist", columnName), "columnName");
            var o = _dataReader.GetValue(i);
            return (o is T) ? (T)o : defaultValue;
        }


        /// <summary>
        /// Tries to coerce column to T
        /// </summary>
        /// <typeparam name="T">Desired type</typeparam>
        /// <param name="i">Zero-based index of ordinal</param>
        /// <param name="value">Returned column value</param>
        /// <returns>True if column is of type t and not null</returns>
        public bool TryGet<T>(int i, out T value)
        {
            var o = _dataReader.GetValue(i);
            bool result = (o is T);
            value = result ? (T)o : default(T);
            return result;
        }


        /// <summary>
        /// Tries to coerce column to T
        /// </summary>
        /// <typeparam name="T">Desired type</typeparam>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returned column value</param>
        /// <returns>True if column is of type t and not null</returns>
        public bool TryGet<T>(string columnName, out T value)
        {
            int i = _dataReader.GetOrdinal(columnName);
            if (i == -1)
                throw new ArgumentException(string.Format("Column '{0}' does not exist", columnName), "columnName");

            var o = _dataReader.GetValue(i);
            bool result = (o is T);
            value = result ? (T)o : default(T);
            return result;
        }


        /// <summary>
        /// Returns a nullable scalar value
        /// </summary>
        /// <typeparam name="T">Scalar type</typeparam>
        /// <param name="i">Zero-based column ordinal</param>
        /// <returns>Value of column</returns>
        public T? GetNullable<T>(int i) where T: struct
        {
            T? result;
            var o = _dataReader.GetValue(i);
            if (o == DBNull.Value)
                result = null;
            else
                result = (T)o;
            return result;
        }

        
        /// <summary>
        /// Returns a nullable scalar value
        /// </summary>
        /// <typeparam name="T">Scalar type</typeparam>
        /// <param name="columnName">Column name</param>
        /// <returns>Value of column</returns>
        public T? GetNullable<T>(string columnName) where T : struct
        {
            int i = _dataReader.GetOrdinal(columnName);
            if (i == -1)
                throw new ArgumentException(string.Format("Column '{0}' does not exist", columnName), "columnName");
            T? result;
            var o = _dataReader.GetValue(i);
            if (o == DBNull.Value)
                result = null;
            else
                result = (T)o;
            return result;
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public bool GetBoolean(int i)
        {
            return _dataReader.GetBoolean(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a nullable boolean
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or boolean value</returns>
        public bool? GetNullableBoolean(int i)
        {
            return GetNullable<bool>(i);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public byte GetByte(int i)
        {
            return _dataReader.GetByte(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a nullable byte
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or byte value</returns>
        public byte? GetNullableByte(int i)
        {
            return GetNullable<byte>(i);
        }


        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer
        /// as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
        /// <param name="bufferoffset">The index for buffer to start the read operation.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through System.Data.IDataRecord.FieldCount.</exception>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

       
        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public char GetChar(int i)
        {
            return _dataReader.GetChar(i);
        }


        /// <summary>
        /// Gets the value of the specified column as a nullable char
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or char value</returns>
        public char? GetNullableChar(int i)
        {
            return GetNullable<char>(i);
        }


        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer
        ///     as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <param name="fieldoffset">The index within the row from which to start the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
        /// <param name="bufferoffset">The index for buffer to start the read operation.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }


        /// <summary>
        /// Returns an System.Data.IDataReader for the specified column ordinal.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>An System.Data.IDataReader.</returns>
        public IDataReader GetData(int i)
        {
            return _dataReader.GetData(i);
        }


        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The data type information for the specified field.</returns>
        public string GetDataTypeName(int i)
        {
            return _dataReader.GetDataTypeName(i);
        }

        /// <summary>
        /// Gets the UTC-DateTime from specified field
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        public DateTime GetUtc(int i)
        {
            return DateTime.SpecifyKind(_dataReader.GetDateTime(i), DateTimeKind.Utc);
        }


        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        public DateTime GetDateTime(int i)
        {
            return _dataReader.GetDateTime(i);
        }


        /// <summary>
        /// Gets the value of the specified column as a nullable DateTime
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or DateTime value</returns>
        public DateTime? GetNullableDateTime(int i)
        {
            return GetNullable<DateTime>(i);
        }


        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        public decimal GetDecimal(int i)
        {
            return _dataReader.GetDecimal(i);
        }


        /// <summary>
        /// Gets the value of the specified column as a nullable decimal
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or decimal value</returns>
        public Decimal? GetNullableDecimal(int i)
        {
            return GetNullable<Decimal>(i);
        }


        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The double-precision floating point number of the specified field.</returns>
        public double GetDouble(int i)
        {
            return _dataReader.GetDouble(i);
        }

        
        /// <summary>
        /// Gets the value of the specified column as a nullable double
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or double value</returns>
        public double? GetNullableDouble(int i)
        {
            return GetNullable<double>(i);
        }


        /// <summary>
        /// Gets the System.Type information corresponding to the type of System.Object
        ///    that would be returned from System.Data.IDataRecord.GetValue(System.Int32).
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>
        /// The System.Type information corresponding to the type of System.Object that
        ///     would be returned from System.Data.IDataRecord.GetValue(System.Int32).
        /// </returns>
        public Type GetFieldType(int i)
        {
            return _dataReader.GetFieldType(i);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The single-precision floating point number of the specified field.</returns>
        public float GetFloat(int i)
        {
            return _dataReader.GetFloat(i);
        }


        /// <summary>
        /// Gets the value of the specified column as a nullable float
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or float value</returns>
        public float? GetNullableFloat(int i)
        {
            return GetNullable<float>(i);
        }



        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find</param>
        /// <returns>The GUID value of the specified field.</returns>
        public Guid GetGuid(int i)
        {
            return _dataReader.GetGuid(i);
        }


        /// <summary>
        /// Gets the value of the specified column as a nullable guid
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or Guid value</returns>
        public Guid? GetNullableGuid(int i)
        {
            return GetNullable<Guid>(i);
        }


        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The 16-bit signed integer value of the specified field.</returns>
        public short GetInt16(int i)
        {
            return _dataReader.GetInt16(i);
        }

        
        /// <summary>
        /// Gets the value of the specified column as a nullable Int16
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or Int16 value</returns>
        public Int16? GetNullableInt16(int i)
        {
            return GetNullable<Int16>(i);
        }


        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        public int GetInt32(int i)
        {
            return _dataReader.GetInt32(i);
        }

        
        /// <summary>
        /// Gets the value of the specified column as a nullable Int32
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or Int32 value</returns>
        public Int32? GetNullableInt32(int i)
        {
            return GetNullable<Int32>(i);
        }


        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        public long GetInt64(int i)
        {
            return _dataReader.GetInt64(i);
        }

        
        /// <summary>
        /// Gets the value of the specified column as a nullable Int64
        /// </summary>
        /// <param name="i">The zero-based column ordinal.</param>
        /// <returns>Null or Int64 value</returns>
        public Int64? GetNullableInt64(int i)
        {
            return GetNullable<Int64>(i);
        }



        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The name of the field or the empty string (""), if there is no value to return.</returns>
        public string GetName(int i)
        {
            return _dataReader.GetName(i);
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name">The name of the field to find.</param>
        /// <returns>The index of the named field.</returns>
        public int GetOrdinal(string name)
        {
            return _dataReader.GetOrdinal(name);
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The string value of the specified field; null if dbnull was returned.</returns>
        public string GetString(int i)
        {
            return _dataReader.GetValue(i) as string;
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>The System.Object which will contain the field value upon return.</returns>
        public object GetValue(int i)
        {
            return _dataReader.GetValue(i);
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values">An array of System.Object to copy the attribute fields into.</param>
        /// <returns>The number of instances of System.Object in the array.</returns>
        public int GetValues(object[] values)
        {
            return _dataReader.GetValues(values);
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="i">The index of the field to find.</param>
        /// <returns>true if the specified field is set to null; otherwise, false.</returns>
        public bool IsDBNull(int i)
        {
            return _dataReader.IsDBNull(i);
        }

        /// <summary>
        /// Closes the reader object and closes the connection
        /// </summary>
        public void Close()
        {
            Dispose();
        }


        /// <summary>
        /// Get the int value of the specified field.
        /// </summary>
        /// <param name="i">The inde of the field to find.</param>
        /// <returns>Integer value of the specified field.</returns>
        public int GetInt(int i)
        {
            return _dataReader.GetInt32(i);
        }

        /// <summary>
        /// Returns true if current column is dbnull, does not advance pointer
        /// </summary>
        /// <returns>True if current column is null</returns>
        public bool IsDBNull()
        {
            return _dataReader.IsDBNull(_columnIndex);
        }

        #region Next Methods

        /// <summary>
        /// Returns next column
        /// </summary>
        /// <returns>Column as object</returns>
        public object Next()
        {
            return _dataReader[_columnIndex++];
        }


        /// <summary>
        /// Returns next column, coercing to T
        /// </summary>
        /// <returns>Column as T</returns>
        public T Next<T>()
        {
            return (T)_dataReader[_columnIndex++];
        }      


        /// <summary>
        /// Return next column as T, or as default of T if null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T NextSafe<T>()
        {
            var o = _dataReader[_columnIndex++];
            return (o is T) ? (T)o : default(T);
        }


        /// <summary>
        /// Return next column as T, or as null if null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? NextNullable<T>() where T : struct
        {
            var o = _dataReader[_columnIndex++];
            return (o is T) ? (T)o : (T?)null;
        }


        /// <summary>
        /// Returns the next column as a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] NextBlob()
        {
            return _dataReader[_columnIndex++] as byte[];
        }

		
		/// <summary>
		/// Returns next column as a bool
		/// </summary>
		public bool NextBoolean()
		{
			return _dataReader.GetBoolean(_columnIndex++);
		}


		/// <summary>
		/// Returns the next column as an int
		/// </summary>
		/// <returns></returns>
		public int NextInt()
        {
            return _dataReader.GetInt32(_columnIndex++);
        }


        /// <summary>
        /// Returns the next column as an Int32
        /// </summary>
        /// <returns></returns>
        public Int16 NextInt16()
        {
            return _dataReader.GetInt16(_columnIndex++);
        }


        /// <summary>
        /// Returns the next column as an Int32
        /// </summary>
        /// <returns></returns>
        public Int32 NextInt32()
        {
            return _dataReader.GetInt32(_columnIndex++);
        }


        /// <summary>
        /// Returns the next column as an Int32
        /// </summary>
        /// <returns></returns>
        public Int64 NextInt64()
        {
            return _dataReader.GetInt64(_columnIndex++);
        }


        /// <summary>
        /// Returns the next column as a Guid
        /// </summary>
        /// <returns></returns>
        public Guid NextGuid()
        {
            return _dataReader.GetGuid(_columnIndex++);
        }
        
        /// <summary>
        /// Returns the next column as a string
        /// </summary>
        /// <returns></returns>
        public string NextString()
        {
            return GetString(_columnIndex++);
        }

        /// <summary>
        /// Returns the next column as a long
        /// </summary>
        /// <returns>Column value as long</returns>
        public long NextLong()
        {
            return _dataReader.GetInt64(_columnIndex++);
        }


        /// <summary>
        /// Returns the next column as a double
        /// </summary>
        /// <returns>Column value as double</returns>
        public double NextDouble()
        {
            return _dataReader.GetDouble(_columnIndex++);
        }

        /// <summary>
        /// Returns the next column as float
        /// </summary>
        /// <returns>Column value as float</returns>
        public float NextFloat()
        {
            return _dataReader.GetFloat(_columnIndex++);
        }

        /// <summary>
        /// Returns next column as DateTime
        /// </summary>
        /// <returns>Column value as DateTime</returns>
        public DateTime NextDateTime()
        {
            return _dataReader.GetDateTime(_columnIndex++);
        }

        /// <summary>
        /// Returns next column as UTC DateTime
        /// </summary>
        /// <returns>Column value as DateTime</returns>
        public DateTime NextUtc()
        {
            return DateTime.SpecifyKind(_dataReader.GetDateTime(_columnIndex++), DateTimeKind.Utc);
        }


        /// <summary>
        /// Returns next column as Decimal
        /// </summary>
        /// <returns>Column value as Decimal</returns>
        public Decimal NextDecimal()
        {
            return _dataReader.GetDecimal(_columnIndex++);
        }


        /// <summary>
        /// Skips current column
        /// </summary>
        public void Skip()
        {
			_columnIndex++;
        }

		/// <summary>
		/// Skips current column
		/// </summary>
		public void Skip(int count)
		{
			_columnIndex += count;
		}

		#endregion

		/// <summary>
		/// Returns string from embedded reader
		/// </summary>
		/// <returns></returns>
		public override string ToString()
        {
            return _dataReader.ToString();
        }

        #region Column Methods

        /// <summary>
        /// Returns values from single column 'i' as T
        /// </summary>
        public IEnumerable<T> Column<T>(int index)
        {
            while (Read())
                yield return (T)this[index];
            Dispose();
        }

        /// <summary>
        /// Returns values from single column 'name' as T
        /// </summary>
        public IEnumerable<T> Column<T>(string name)
        {
            var index = GetOrdinal(name);
            return (index > 0) ? Column<T>(index) : Enumerable.Empty<T>();
        }

        #endregion

        #region ToObjects Methods

        /// <summary>
        /// Returns a sequence of objects from a database reader
        /// </summary>
        public IEnumerable<T> ToObjects<T>(Func<Reader, T> creator)
        {
			if (creator == null)
				throw new ArgumentNullException(nameof(creator));

            while (Read())
                yield return creator(this);
        }

		/// <summary>
		/// Returns a sequence of objects from a database reader, asynchronously
		/// </summary>
		public async Task<T[]> ToObjectsAsync<T>(Func<Reader, T> creator)
		{
			if (creator == null)
				throw new ArgumentNullException(nameof(creator));

			T[] array = null;
			int length = 0;
			while (await ReadAsync().ConfigureAwait(false))
			{
				if (array == null)
				{
					array = new T[4];
				}
				else if (array.Length == length)
				{
					Array.Resize(ref array, length * 2);
				}
				array[length++] = creator(this);
			}
			if (length == 0)
			{
				array = new T[0];
			}
			else if (length != array.Length)
			{
				Array.Resize(ref array, length);
			}
			return array;
		}


		/// <summary>
		/// Returns one T if one row of data exists in the reader
		/// </summary>
		public T ToObject<T>(Func<Reader, T> creator)
        {
			if (creator == null)
				throw new ArgumentNullException("creator");
            return Read() ? creator(this) : default(T);
        }

		/// <summary>
		/// Returns one T if one row of data exists in the reader
		/// </summary>
		public async Task<T> ToObjectAsync<T>(Func<Reader, T> creator)
		{
			if (creator == null)
				throw new ArgumentNullException(nameof(creator));

			var isFound = await ReadAsync().ConfigureAwait(false);

			return isFound ? creator(this) : default(T);
		}



		/// <summary>
		/// Returns one T if one row of data exists in the reader,
		/// otherwise returns defaultValue
		/// </summary>
		public T ToObject<T>(Func<Reader, T> creator, T defaultValue)
        {
			if (creator == null)
				throw new ArgumentNullException("creator");
            return Read() ? creator(this) : defaultValue;
        }

		#endregion

		/// <summary>
		/// Returns column as a stream (used for Binary and VarBinary columns)
		/// </summary>
		/// <param name="index">Column index</param>
		/// <returns>Stream referencing returned data</returns>
		public Stream GetStream(int index)
		{
			return _dataReader.GetStream(index);
		}

		/// <summary>
		/// Returns next column as a stream (used for Binary and VarBinary columns)
		/// </summary>
		/// <returns>Stream referencing returned data</returns>
		public Stream GetNextStream()
		{
			return _dataReader.GetStream(_columnIndex++);
		}

		/// <summary>
		/// Returns column as a text reader (used for char, nchar, nvarchar, and xml columns)
		/// </summary>
		/// <param name="index"></param>
		/// <returns>TextReader</returns>
		public TextReader GetTextReader(int index)
		{
			return _dataReader.GetTextReader(index);
		}

		/// <summary>
		/// Returns next column as a text reader (used for char, nchar, nvarchar, and xml columns)
		/// </summary>
		/// <returns>TextReader</returns>
		public TextReader GetNextTextReader()
		{
			return _dataReader.GetTextReader(_columnIndex++);
		}

	}
    
    /// <summary>
    /// Reads a row of data from a sql connection and stores in an array that can
    /// can be read one at a time.  We use this class instead of the data reader
    /// reader, so that we don't block other database transactions (i.e. the
    /// reader keeps the connection openConn while the reader is openConn)
    /// </summary>
    public class Row
    {
        /// <summary>
        /// Creates object around an array of data...this method is used to bypass the
        /// database when using a cache
        /// </summary>
        /// <param name="values"></param>
        public Row(object[] values)
        {
            _values = values;
        }


        /// <summary>
        /// If data is present, creates a row object based on the next row
        /// of data found in reader.  Otherwise, returns null
        /// </summary>
        public static Row Create(IDataReader reader)
        {
            Row result;
            if (reader.Read() && (reader.FieldCount > 0))
            {
                object[] values = new object[reader.FieldCount];
                reader.GetValues(values);
                result = new Row(values);
            }
            else
            {
                result = null;
            }
            return result;
        }


		/// <summary>
		/// Creates row asynchronously
		/// </summary>
		public static async Task<Row> CreateAsync(SqlDataReader reader)
		{
			Row result;
			var isOk = await reader.ReadAsync().ConfigureAwait(false);
			if (isOk && (reader.FieldCount > 0))
			{
				var values = new object[reader.FieldCount];
				reader.GetValues(values);
				result = new Row(values);
			}
			else
			{
				result = null;
			}
			return result;
		}


		#region Properties

		#region Values Property
		/// <summary>
		/// Retrieves the row data
		/// </summary>
		public object[] Values
        {
            get { return _values; }
        }
        private readonly object[] _values;
        #endregion

        #region NextIndex
        /// <summary>
        /// Column of next value to retrieve
        /// </summary>
        public int NextIndex
        {
            get { return _nextIndex; }
        }
        private int _nextIndex = 0;
        #endregion

        #endregion

        /// <summary>
        /// Retrieves the next item, converting to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T NextField<T>()
        {
            return (T)_values[_nextIndex++];
        }

        /// <summary>
        /// Retrieves the value at column 'index'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Field<T>(int index)
        {
            return (T)_values[index];
        }

        /// <summary>
        /// Retrieves the next int
        /// </summary>
        /// <returns></returns>
        public int NextInt()
        {
            return (int)_values[_nextIndex++];
        }

        /// <summary>
        /// Retrieves the next string
        /// </summary>
        /// <returns></returns>
        public string NextString()
        {
            return _values[_nextIndex++] as string;
        }

        /// <summary>
        /// Retrieves the next blob / byte array
        /// </summary>
        /// <returns></returns>
        public byte[] NextBlob()
        {
            return _values[_nextIndex++] as byte[];
        }


        /// <summary>
        /// Returns a date from the next column
        /// </summary>
        public DateTime NextDate()
        {
            return (DateTime)_values[_nextIndex++];
        }


        /// <summary>
        /// Displays next read value (for debug purposes)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}",
                _nextIndex, _values[_nextIndex]);
        }
    }
    
    /// <summary>
    /// Facilitates getting data from SQL databases.
    /// </summary>
    [Serializable]
    public class SqlDb:   IDisposable
    {
        private bool _isDisposed;
        private SqlTransaction _transaction;

        #region Constructors

        /// <summary>
        /// Creates a MSSql connection from a connection string
        /// </summary>
        public SqlDb(string connString)
        {
            if (string.IsNullOrEmpty(connString))
                throw new ArgumentNullException(nameof(connString));
            _connection = new SqlConnection(connString);
        }

        /// <summary>
        /// Creates a MSSql connection on server 'server' using 'catalog' as the
        /// initial catalog.  Security is set to Windows Integrated Security.
        /// </summary>
        public SqlDb(string server, string catalog, bool integratedSecurity = true)
        {
            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = server,
                InitialCatalog = catalog,
                IntegratedSecurity = integratedSecurity
            };
            _connection = new SqlConnection(builder.ToString());
        }


        /// <summary>
        /// Creates a MsSql connection from the specified database address and
        /// username password
        /// </summary>
        public SqlDb(string dbAddress, string userName, string password)
        {
            var builder = new SqlConnectionStringBuilder(dbAddress)
            {
                IntegratedSecurity = false,
                UserID = userName,
                Password = password
            };
            _connection = new SqlConnection(builder.ToString());
        }


        /// <summary>
        /// Creates a MsSql connection from the specified server, catalog, 
        /// network library, username, and password
        /// </summary>
        /// <param name="server">Server and port</param>
        /// <param name="catalog">Name of database</param>
        /// <param name="networkLibrary">The network transport library with which to connect</param>
        /// <param name="userName">The username with which to authenticate</param>
        /// <param name="password">The password with which to authenticate</param>
        public SqlDb(string server, string catalog, string networkLibrary, string userName, string password)
        {
            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = server,
                InitialCatalog = catalog,
                IntegratedSecurity = false,
                UserID = userName,
                Password = password,
                NetworkLibrary = networkLibrary
            };
            _connection = new SqlConnection(builder.ToString());
        }


        /// <summary>
        /// Creates a SqlDb object from the specified database server,
        /// catalog, username, and password.  
        /// </summary>
        public SqlDb(string server, string catalog, string userName, string password)
        {
            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = server,
                InitialCatalog = catalog,
                IntegratedSecurity = false,
                UserID = userName,
                Password = password
            };
            _connection = new SqlConnection(builder.ToString());
        }

        /// <summary>
        /// Creates a SqlDb object from the provided connection, taking ownership of the connection
        /// </summary>
        /// <param name="connection">Sql connection</param>
        public SqlDb(SqlConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public SqlDb(SqlDb cpy)
        {
            _connection = (SqlConnection)(cpy._connection as ICloneable).Clone();
        }

        
		#endregion

        /// <summary>
        /// Creates a copy of this object
        /// </summary>
        /// <returns></returns>
        public SqlDb Clone()
        {
            return new SqlDb(this);
        }

		/// <summary>
		/// Disposes the connection
		/// </summary>
		public void Dispose()
		{
			if (!_isDisposed)
			{
				if (_transaction != null)
				{
					_transaction.Dispose();
					_transaction = null;
				}
				_connection.Dispose();
				_isDisposed = true;
			}
		}

		#region Properties

		#region Connection Property
		/// <summary>
		/// Returns the SQL connection object
		/// </summary>
		public SqlConnection Connection
        {
            get { return _connection; }
        }

        [NonSerialized]
        private SqlConnection _connection;
        #endregion

        #region Timeout Property
        /// <summary>
        /// Timeout in seconds used for stored procedure/query execution
        /// (Default = 10 minutes)
        /// </summary>
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
        private int _timeout = 600 /*secs*/;
        #endregion

        #endregion

        /// <summary>
        /// If not under a transaction, opens connection
        /// </summary>
		/// <returns>Returns true if connection was newly opened, false if already opened</returns>
        protected bool OpenConnection()
        {
			var result = false;
			if (_connection.State == ConnectionState.Closed)
			{
				_connection.Open();
				result = true;
			}
			return result;
        }

		/// <summary>
		/// If not under a transaction, opens connection asynchronously
		/// </summary>
		protected async Task<bool> OpenConnectionAsync()
		{
			var result = false;
			if (_connection.State == ConnectionState.Closed)
			{
				await _connection.OpenAsync().ConfigureAwait(false);
				result = true;
			}
			return result;
		}

        /// <summary>
        /// If not under a transaction, closes connection
        /// </summary>
        protected void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
        }

        #region Command Methods

        #region CreateSpc Methods
        /// <summary>
        /// Creates a SqlCommand object for the stored procedured 'spcName'
        /// </summary>
        public SqlCommand CreateSpc(string spcName)
        {
			if (string.IsNullOrEmpty(spcName))
				throw new ArgumentNullException("spcName");

			return new SqlCommand(spcName, _connection)
            {
                CommandType = CommandType.StoredProcedure,
                Transaction = _transaction,
                CommandTimeout = _timeout /*secs*/
            };
        }
        
        /// <summary>
        /// Creates an SqlCommand object for the stored procedure specified by name.
        /// </summary>
        /// <param name="spcName">Name of the stored procedure</param>
        /// <param name="args">Pairs of stored procedure parameters:  1st is the name, 2nd is the value</param>
        /// <returns>SqlCommand</returns>
        public SqlCommand CreateSpc(string spcName, params object[] args)
        {
            var cmd = CreateSpc(spcName);
			if (args == null)
				throw new ArgumentNullException("parameters");

            for (int i = 0; i + 1 < args.Length; i += 2)
            {
                var name = args[i].ToString();
                var value = args[i + 1];
                var parameter = ToParameter(name, value);
                cmd.Parameters.Add(parameter);
            }

            return cmd;
        }


		#endregion

		#region CreateQuery Methods
		/// <summary>
		/// Creates a SqlCommand prepared for a query
		/// </summary>
		/// <param name="query">Query text</param>
		/// <returns>SqlCommand</returns>
		public SqlCommand CreateQuery(string query)
		{
			if (string.IsNullOrEmpty(query))
				throw new ArgumentNullException("query");
			return new SqlCommand(query, _connection)
			{
				CommandType = CommandType.Text,
				CommandTimeout = _timeout, /*secs*/
				Transaction = _transaction
			};
		}

		/// <summary>
		/// Creates a SqlCommand for a query
		/// </summary>
		/// <param name="query">Query text (parameters are specified using @0, @1)</param>
		/// <param name="args">Query parameters</param>
		/// <returns>SqlCommand</returns>
		public SqlCommand CreateQuery(string query, params object[] args)
		{
			if (args == null)
				throw new ArgumentNullException(nameof(args));
			var cmd = CreateQuery(query);

			for (int i = 0; i < args.Length; i++)
			{
				var name = string.Format("@{0}", i);
				var value = args[i];
				var sqlParam = ToParameter(name, value);
				cmd.Parameters.Add(sqlParam);
			}

			return cmd;
		}

		#endregion

		#region Scalar Methods
		/// <summary>
		/// Executes command, casts and returns the first returned data item
		/// </summary>
		public T Scalar<T>(SqlCommand command, T defaultValue)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			T result;
			try
			{
				var hasOpened = OpenConnection();
				try
				{
					var obj = command.ExecuteScalar();
					if ((obj != null) && (obj is T))
						result = (T)obj;
					else
						result = defaultValue;
				}
				finally
				{
					if (hasOpened)
						CloseConnection();
				}
			}
			catch (SqlException ex)
			{
                throw DatabaseException.Create(ex);
			}
			catch (InvalidOperationException ex)
			{
				throw new ConnectionException(ex);
			}
			return result;
		}


		/// <summary>
		/// Executes command asynchronously, casts, and returns first returned data item
		/// </summary>
		public async Task<T> ScalarAsync<T>(SqlCommand command, T defaultValue)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			T result;
			try
			{
				await OpenConnectionAsync().ConfigureAwait(false);
				try
				{
					var obj = await command.ExecuteScalarAsync().ConfigureAwait(false);
					if ((obj != null) && (obj is T))
						result = (T)obj;
					else
						result = defaultValue;
				}
				finally
				{
					CloseConnection();
				}
			}
			catch (SqlException ex)
			{
                throw DatabaseException.Create(ex);
            }
            catch (InvalidOperationException ex)
			{
				throw new ConnectionException(ex);
			}
			return result;
		}

		
		/// <summary>
		/// Executes command, casts and returns the first returned data item
		/// </summary>
		public bool TryScalar<T>(SqlCommand command, out T value)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			var result = false;
			value = default(T);

			try
			{
				var hasOpened = OpenConnection();
				try
				{
					object obj = command.ExecuteScalar();
					if ((obj != null) && (obj is T))
					{
						value = (T)obj;
						result = true;
					}
				}
				finally
				{
					if (hasOpened)
						CloseConnection();
				}
			}
			catch (SqlException ex)
			{
                throw DatabaseException.Create(ex);
            }
            catch (InvalidOperationException ex)
			{
				throw new ConnectionException(ex);
			}
			return result;
		}

		#endregion

		#region Row Methods
		/// <summary>
		/// Executes command and returns the first row of the first table in the Row
		/// helper object.  This method is a more lightweight than DataRow as
		/// it only stores the field values (not names, nor link to original DataTable)
		/// </summary>
		public Row Row(SqlCommand command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			var hasOpened = OpenConnection();
			try
			{
				using (var rdr = command.ExecuteReader())
					return Maple.Sql.Row.Create(rdr);
			}
			finally
			{
				if (hasOpened)
					CloseConnection();
			}
		}


		/// <summary>
		/// Executes command asynchronously and returns the first row of the first table 
		/// in the Row helper object.  This method is a more lightweight than DataRow as
		/// it only stores the field values (not names, nor link to original DataTable)
		/// </summary>
		public async Task<Row> RowAsync(SqlCommand command)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));

			var hasOpened = await OpenConnectionAsync().ConfigureAwait(false);
			try
			{
				var rdr = await command.ExecuteReaderAsync().ConfigureAwait(false);
				using (rdr)
					return await Sql.Row.CreateAsync(rdr).ConfigureAwait(false);
			}
			finally
			{
				if (hasOpened)
					CloseConnection();
			}
		}

		#endregion

		#region XmlReader Methods
		/// <summary>
		/// Executes command returning the results as an XmlDocument
		/// </summary>
		public XmlReader XmlReader(SqlCommand command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			var hasOpened = OpenConnection();
			try
			{
				return command.ExecuteXmlReader();
			}
			finally
			{
				if (hasOpened)
					CloseConnection();
			}
		}


		/// <summary>
		/// Executes command returning the results as an XmlReader
		/// </summary>
		public async Task<XmlReader> XmlReaderAsync(SqlCommand command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			var hasOpened = await OpenConnectionAsync().ConfigureAwait(false);
			try
			{
				return await command.ExecuteXmlReaderAsync().ConfigureAwait(false);
			}
			finally
			{
				if (hasOpened)
					CloseConnection();
			}
		}

		#endregion

		#region NonQuery Methods

		/// <summary>
		/// Executes command, returning number of rows affected
		/// </summary>
		/// <param name="command">Command to execute</param>
		/// <returns>Number of rows affected</returns>
		public int NonQuery(SqlCommand command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			var hasOpened = OpenConnection();
			try
			{
				return command.ExecuteNonQuery();
			}
			finally
			{
				if (hasOpened)
					CloseConnection();
			}
		}

		/// <summary>
		/// Executes command asynchronously, returning number of rows affected
		/// </summary>
		/// <param name="command">Command to execute</param>
		/// <returns>Number of rows affected</returns>
		public async Task<int> NonQueryAsync(SqlCommand command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			var hasOpened = await OpenConnectionAsync().ConfigureAwait(false);
			try
			{
				return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
			}
			finally
			{
				if (hasOpened)
					CloseConnection();
			}
		}

		#endregion

		#endregion

		#region Query Method

		#region Query Reader Methods

		/// <summary>
		/// Executes query and returns a Reader with the result
		/// </summary>
		public Reader Query(string query, params object[] args)
		{
			var command = CreateQuery(query, args);
			return new Reader(command);
		}

		
		/// <summary>
		/// Executes query and returns the result as a Reader
		/// </summary>
		public Reader Query(string query)
		{
			var cmd = CreateQuery(query);
			return new Reader(cmd);
		}


		/// <summary>
		/// Executes query asynchronously
		/// </summary>
		public async Task<Reader> QueryAsync(string query, params object[] args)
		{
			var cmd = CreateQuery(query, args);
			return await Reader.CreateAsync(cmd).ConfigureAwait(false);
		}

		/// <summary>
		/// Executes query asynchronously
		/// </summary>
		public async Task<Reader> QueryAsync(string query)
		{
			var cmd = CreateQuery(query);
			return await Reader.CreateAsync(cmd).ConfigureAwait(false);
		}


		#endregion

		#region QueryRow Methods
		/// <summary>
		/// Executes query, returning the first row of the first table.
		/// This method is a little more efficient then DataRow since it doesn't
		/// have the overhead of a DataTable.
		/// </summary>
		public Row QueryRow(string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return Row(cmd);
		}


		/// <summary>
		/// Returns the first row of the first table returned by the query
		/// This method is a little more efficient then DataRow since it doesn't
		/// have the overhead of a DataTable.
		/// </summary>
		public Row QueryRow(string query)
		{
			using (var cmd = CreateQuery(query))
				return Row(cmd);
		}

		/// <summary>
		/// Performs the QueryRow asynchronously
		/// </summary>
		public async Task<Row> QueryRowAsync(string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return await RowAsync(cmd).ConfigureAwait(false);
		}

		/// <summary>
		/// Performs the QueryRow asynchronously
		/// </summary>
		public async Task<Row> QueryRowAsync(string query)
		{
			using (var cmd = CreateQuery(query))
				return await RowAsync(cmd).ConfigureAwait(false);
		}

		#endregion

		#region QueryScalar<T> Method

		/// <summary>
		/// Executes query, casts, and returns the first returned data item
		/// Casts and returns the first data item returned by the stored procedure
		/// </summary>
		public T QueryScalar<T>(T defaultValue, string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return Scalar<T>(cmd, defaultValue);
		}


		/// <summary>
		/// Executes query, casts, and returns the first returned data item
		/// </summary>
		public T QueryScalar<T>(T defaultValue, string query)
		{
			using (var cmd = CreateQuery(query))
				return Scalar<T>(cmd, defaultValue);
		}


		/// <summary>
		/// Executes query asynchronously, casts, and returns the first returned data item
		/// Casts and returns the first data item returned by the stored procedure
		/// </summary>
		public async Task<T> QueryScalarAsync<T>(T defaultValue, string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return await ScalarAsync<T>(cmd, defaultValue).ConfigureAwait(false);
		}


		/// <summary>
		/// Executes query asynchronously, casts, and returns the first returned data item
		/// </summary>
		public async Task<T> QueryScalarAsync<T>(T defaultValue, string query)
		{
			using (var cmd = CreateQuery(query))
				return await ScalarAsync<T>(cmd, defaultValue).ConfigureAwait(false);
		}


		/// <summary>
		/// Executes query, casts, and returns first returned data item
		/// </summary>
		public bool TryQueryScalar<T>(out T value, string query)
		{
			using (var cmd = CreateQuery(query))
				return TryScalar<T>(cmd, out value);
		}

		/// <summary>
		/// Executes query, casts, and returns first returned data item
		/// </summary>
		public bool TryQueryScalar<T>(out T value, string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return TryScalar<T>(cmd, out value);
		}

		#endregion

		#region NonQuery Methods

		/// <summary>
		/// Executes query, returning the number of rows returned
		/// </summary>
		public int NonQuery(string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return NonQuery(cmd);
		}


		/// <summary>
		/// Executes query, returning the number of rows returned
		/// </summary>
		public int NonQuery(string query)
		{
			using (var cmd = CreateQuery(query))
				return NonQuery(cmd);
		}


		/// <summary>
		/// Executes query asynchronously, returning the number of rows returned
		/// </summary>
		public async Task<int> NonQueryAsync(string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return await NonQueryAsync(cmd).ConfigureAwait(false);
		}


		/// <summary>
		/// Executes query asynchronously, returning the number of rows returned
		/// </summary>
		public async Task<int> NonQueryAsync(string query)
		{
			using (var cmd = CreateQuery(query))
				return await NonQueryAsync(cmd).ConfigureAwait(false);
		}

		#endregion

		#region QueryXml Method

		/// <summary>
		/// Returns an XML document result from query
		/// </summary>
		public XmlReader QueryXml(string query)
		{
			using (var cmd = CreateQuery(query))
				return XmlReader(cmd);
		}


		/// <summary>
		/// Returns an XML document result from query
		/// </summary>
		public XmlReader QueryXml(string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return XmlReader(cmd);
		}

		/// <summary>
		/// Returns an XML document result from query
		/// </summary>
		public async Task<XmlReader> QueryXmlAsync(string query)
		{
			using (var cmd = CreateQuery(query))
				return await XmlReaderAsync(cmd).ConfigureAwait(false);
		}


		/// <summary>
		/// Returns an XML document result from query
		/// </summary>
		public async Task<XmlReader> QueryXmlAsync(string query, params object[] args)
		{
			using (var cmd = CreateQuery(query, args))
				return await XmlReaderAsync(cmd).ConfigureAwait(false);
		}


		#endregion

		#region Query to objects methods

		/// <summary>
		/// Executes query and returns a sequence of T-object
		/// </summary>
		public IEnumerable<T> Query<T>(Func<Reader, T> creator, string query, params object[] args)
		{
			using (var reader = Query(query, args))
			{
				foreach (var obj in reader.ToObjects(creator))
					yield return obj;
			}
		}

		/// <summary>
		/// Executes query and returns a sequence of T-object
		/// </summary>
        /// <param name="creator">Creates T from row returned from reader</param>
        /// <param name="query">Query text</param>
        /// <returns>Collection of T objects</returns>
		public IEnumerable<T> Query<T>(Func<Reader, T> creator, string query)
		{
			using (var reader = Query(query))
			{
				foreach (var obj in reader.ToObjects(creator))
					yield return obj;
			}
		}

		/// <summary>
		/// Executes query asynchronously, returns a sequence of T-objects
		/// </summary>
		public async Task<T[]> QueryAsync<T>(Func<Reader, T> creator, string query, params object[] args)
		{
			using (var reader = await QueryAsync(query, args).ConfigureAwait(false))
				return await reader.ToObjectsAsync(creator).ConfigureAwait(false);
		}


		/// <summary>
		/// Executes query asynchronously, returns a sequence of T-objects
		/// </summary>
		public async Task<T[]> QueryAsync<T>(Func<Reader, T> creator, string query)
		{
			using (var reader = await QueryAsync(query))
				return await reader.ToObjectsAsync(creator).ConfigureAwait(false);
		}

		#endregion

		#region QueryFirst Method

		/// <summary>
		/// Returns first row of query as object generatord from creator
		/// </summary>
		public T QueryFirst<T>(Func<Reader, T> creator, string query, params object[] args)
		{
			using (var reader = Query(query, args))
				return reader.ToObject(creator);
		}

		/// <summary>
		/// Returns first row of query as object generatord from creator
		/// </summary>
		public T QueryFirst<T>(Func<Reader, T> creator, string query)
		{
			using (var reader = Query(query))
				return reader.ToObject(creator);
		}

		/// <summary>
		/// Returns first row of query as object generatord from creator
		/// </summary>
		public async Task<T> QueryFirstAsync<T>(Func<Reader, T> creator, string query, params object[] args)
		{
			using (var reader = await QueryAsync(query, args).ConfigureAwait(false))
				return await reader.ToObjectAsync(creator).ConfigureAwait(false);
		}

		/// <summary>
		/// Returns first row of query as object generatord from creator
		/// </summary>
		public async Task<T> QueryFirstAsync<T>(Func<Reader, T> creator, string query)
		{
			using (var reader = await QueryAsync(query).ConfigureAwait(false))
				return await reader.ToObjectAsync(creator).ConfigureAwait(false);
		}


		#endregion

		#endregion

		#region Stored Procedure Methods

		#region Exec Method
		/// <summary>
		/// Returns the Reader wrapper result from the stored procedure call specified
		/// by spcName and parameters.
		/// </summary>
		public Reader Exec(string spcName, params object[] parameters)
		{
			return new Reader(CreateSpc(spcName, parameters));
		}

		/// <summary>
		/// Returns the Reader wrapper result from the stored procedure call specified
		/// by spcName and parameters.
		/// </summary>
		public Reader Exec(string spcName)
		{
			return new Reader(CreateSpc(spcName));
		}

		/// <summary>
		/// Returns the Reader wrapper result from the stored procedure call specified
		/// by spcName and parameters.
		/// </summary>
		public async Task<Reader> ExecAsync(string spcName, params object[] parameters)
		{
			return await Reader.CreateAsync(CreateSpc(spcName, parameters))
				.ConfigureAwait(false);
		}

		/// <summary>
		/// Returns the Reader wrapper result from the stored procedure call specified
		/// by spcName and parameters.
		/// </summary>
		public async Task<Reader> ExecAsync(string spcName)
		{
			return await Reader.CreateAsync(CreateSpc(spcName)).ConfigureAwait(false);
		}

		#endregion

		#region ExecCommand

		/// <summary>
		/// Calls stored procedure spcName, returning the number of rows returned
		/// </summary>
		public int ExecCommand(string spcName, params object[] args)
		{
			using (var cmd = CreateSpc(spcName, args))
			{
				try
				{
					return NonQuery(cmd);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
							spcName, e.Message), e);
				}
			}
		}

		/// <summary>
		/// Calls stored procedure spcName, returning the number of rows returned
		/// </summary>
		public int ExecCommand(string spcName)
		{
			using (var cmd = CreateSpc(spcName))
			{
				try
				{
					return NonQuery(cmd);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
							spcName, e.Message), e);
				}
			}
		}

		/// <summary>
		/// Calls stored procedure spcName asynchronously, returning the number of rows returned
		/// </summary>
		public async Task<int> ExecCommandAsync(string spcName, params object[] args)
		{
			using (var cmd = CreateSpc(spcName, args))
			{
				try
				{
					return await NonQueryAsync(cmd).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
							spcName, e.Message), e);
				}
			}
		}

		
		/// <summary>
		/// Calls stored procedure spcName, returning the number of rows returned
		/// </summary>
		public async Task<int> ExecCommandAsync(string spcName)
		{
			using (var cmd = CreateSpc(spcName))
			{
				try
				{
					return await NonQueryAsync(cmd).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
							spcName, e.Message), e);
				}
			}
		}

		
		#endregion

		#region ExecRow Methods
		
		/// <summary>
		/// Returns the first row of the first table returned by the stored procedure 
		/// spc_name wrapped in the Row helper object.
		/// This method is a little more efficient then DataRow since it doesn't
		/// have the overhead of a DataTable.
		/// </summary>
		public Row ExecRow(string spcName, params object[] parameters)
		{
			try
			{
				using (var cmd = CreateSpc(spcName, parameters))
					return Row(cmd);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
						spcName, e.Message), e);
			}
		}

		/// <summary>
		/// Returns the first row of the first table returned by the stored procedure 
		/// spc_name wrapped in the Row helper object.
		/// This method is a little more efficient then DataRow since it doesn't
		/// have the overhead of a DataTable.
		/// </summary>
		public Row ExecRow(string spcName)
		{
			try
			{
				using (var cmd = CreateSpc(spcName))
					return Row(cmd);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
						spcName, e.Message), e);
			}
		}

		/// <summary>
		/// Returns the first row of the first table returned by the stored procedure 
		/// spc_name wrapped in the Row helper object.
		/// This method is a little more efficient then DataRow since it doesn't
		/// have the overhead of a DataTable.
		/// </summary>
		public async Task<Row> ExecRowAsync(string spcName, params object[] parameters)
		{
			try
			{
				using (var cmd = CreateSpc(spcName, parameters))
					return await RowAsync(cmd).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
						spcName, e.Message), e);
			}
		}

		/// <summary>
		/// Returns the first row of the first table returned by the stored procedure 
		/// spc_name wrapped in the Row helper object.
		/// This method is a little more efficient then DataRow since it doesn't
		/// have the overhead of a DataTable.
		/// </summary>
		public async Task<Row> ExecRowAsync(string spcName)
		{
			try
			{
				using (var cmd = CreateSpc(spcName))
					return await RowAsync(cmd).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
						spcName, e.Message), e);
			}
		}

		#endregion

		#region ExecXml Method
		/// <summary>
		/// Returns an XML document result from the stored procedure specified by spcName
		/// </summary>
		public XmlReader ExecXml(string spcName, params object[] args)
        {
            try
            {
                using (var cmd = CreateSpc(spcName, args))
                    return XmlReader(cmd);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
                        spcName, e.Message), e);
            }
        }

        
        /// <summary>
        /// Returns an XML document result from the stored procedure specified by spcName
        /// </summary>
        public XmlReader ExecXml(string spcName)
        {
            try
            {
                using (var cmd = CreateSpc(spcName))
                    return XmlReader(cmd);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
                        spcName, e.Message), e);
            }
        }

		/// <summary>
		/// Returns an XML document result from the stored procedure specified by spcName
		/// </summary>
		public async Task<XmlReader> ExecXmlAsync(string spcName, params object[] args)
		{
			try
			{
				using (var cmd = CreateSpc(spcName, args))
					return await XmlReaderAsync(cmd).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
						spcName, e.Message), e);
			}
		}


		/// <summary>
		/// Returns an XML document result from the stored procedure specified by spcName
		/// </summary>
		public async Task<XmlReader> ExecXmlAsync(string spcName)
		{
			try
			{
				using (var cmd = CreateSpc(spcName))
					return await XmlReaderAsync(cmd).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Stored procedure '{0}' error:  {1}",
						spcName, e.Message), e);
			}
		}

		#endregion

		#region Exec (objects) Methods
		/// <summary>
		/// Executes stored procedure 'spcName' and converts the results into a series of
		/// T-objects using 'creator'
		/// </summary>
		public IEnumerable<T> Exec<T>(Func<Reader, T> creator, string spcName, params object[] args)
		{
			using (var rdr = Exec(spcName, args))
			{
				foreach (var item in rdr.ToObjects<T>(creator))
					yield return item;
			}
		}

		/// <summary>
		/// Executes stored procedure 'spcName' and converts the results into a series of
		/// T-objects using 'creator'
		/// </summary>
		public IEnumerable<T> Exec<T>(Func<Reader, T> creator, string spcName)
		{
			using (var rdr = Exec(spcName))
			{
				foreach (var item in rdr.ToObjects<T>(creator))
					yield return item;
			}
		}


		/// <summary>
		/// Executes stored procedure 'spcName' and converts the results into a series of
		/// T-objects using 'creator'
		/// </summary>
		public async Task<T[]> ExecAsync<T>(Func<Reader, T> creator, string spcName, params object[] args)
		{
			using (var rdr = await ExecAsync(spcName, args).ConfigureAwait(false))
				return await rdr.ToObjectsAsync(creator).ConfigureAwait(false);
		}

		/// <summary>
		/// Executes stored procedure 'spcName' and converts the results into a series of
		/// T-objects using 'creator'
		/// </summary>
		public async Task<T[]> ExecAsync<T>(Func<Reader, T> creator, string spcName)
		{
			using (var rdr = await ExecAsync(spcName).ConfigureAwait(false))
				return await rdr.ToObjectsAsync(creator).ConfigureAwait(false);
		}


		/// <summary>
		/// Returns first row of stored procedure as object generatord from creator
		/// </summary>
		public T ExecFirst<T>(Func<Reader, T> creator, string spcName, params object[] args)
		{
			using (var rdr = Exec(spcName, args))
				return rdr.ToObject(creator);
		}

		/// <summary>
		/// Returns first row of stored procedure as object generatord from creator
		/// </summary>
		public T ExecFirst<T>(Func<Reader, T> creator, string spcName)
		{
			using (var rdr = Exec(spcName))
				return rdr.ToObject(creator);
		}


		/// <summary>
		/// Returns first row of stored procedure as object generatord from creator
		/// </summary>
		public async Task<T> ExecFirstAsync<T>(Func<Reader, T> creator, string spcName, params object[] args)
		{
			using (var rdr = await ExecAsync(spcName, args).ConfigureAwait(false))
				return await rdr.ToObjectAsync(creator).ConfigureAwait(false);
		}

		/// <summary>
		/// Returns first row of stored procedure as object generated from creator
		/// </summary>
		public async Task<T> ExecFirstAsync<T>(Func<Reader, T> creator, string spcName)
		{
			using (var rdr = await ExecAsync(spcName).ConfigureAwait(false))
				return await rdr.ToObjectAsync(creator).ConfigureAwait(false);
		}

		#endregion

		#region ExecScalar<T> Method

		/// <summary>
		/// Executes stored procedure, casts, and returns the first returned data item
		/// Casts and returns the first data item returned by the stored procedure
		/// </summary>
		public T ExecScalar<T>(T defaultValue, string spcName, params object[] args)
		{
			using (var cmd = CreateSpc(spcName, args))
				return Scalar<T>(cmd, defaultValue);
		}

		/// <summary>
		/// Executes stored procedure, casts, and returns the first returned data item
		/// Casts and returns the first data item returned by the stored procedure
		/// </summary>
		public T ExecScalar<T>(T defaultValue, string spcName)
		{
			using (var cmd = CreateSpc(spcName))
				return Scalar<T>(cmd, defaultValue);
		}

		/// <summary>
		/// Executes stored procedure asynchronously, casts, and returns the first 
		/// returned data item. 
		/// </summary>
		public async Task<T> ExecScalarAsync<T>(T defaultValue, string spcName, params object[] args)
		{
			using (var cmd = CreateSpc(spcName, args))
				return await ScalarAsync<T>(cmd, defaultValue).ConfigureAwait(false);
		}

		/// <summary>
		/// Executes stored procedure asynchronously, casts, and returns the first 
		/// returned data item
		/// </summary>
		public async Task<T> ExecScalarAsync<T>(T defaultValue, string spcName)
		{
			using (var cmd = CreateSpc(spcName))
				return await ScalarAsync<T>(cmd, defaultValue).ConfigureAwait(false);
		}

		#endregion

		#region TryExecScalar<T> Methods
		/// <summary>
		/// Executes query, casts, and returns first returned data item
		/// </summary>
		public bool TryExecScalar<T>(out T value, string spcName)
		{
			using (var cmd = CreateSpc(spcName))
				return TryScalar<T>(cmd, out value);
		}

		/// <summary>
		/// Executes query, casts, and returns first returned data item
		/// </summary>
		public bool TryExecScalar<T>(out T value, string spcName, params object[] args)
		{
			using (var cmd = CreateSpc(spcName, args))
				return TryScalar<T>(cmd, out value);
		}

		#endregion

		#endregion

		/// <summary>
		/// Executes a script of sql commands separated by semi-colons
		/// </summary>
		/// <param name="script">Set of Sql commands separated by semicolons</param>
		public void ExecScript(string script)
		{
			var segments = script.Split(';');
			var command = new SqlCommand()
			{
				Connection = _connection,
				CommandType = CommandType.Text,
				CommandTimeout = _timeout, /*secs*/
				Transaction = _transaction
			};

			var hasOpened = OpenConnection();

			try
			{
				foreach (var segment in segments)
				{
					if (string.IsNullOrWhiteSpace(segment))
						continue;
					command.CommandText = segment;
					command.ExecuteNonQuery();
				}
			}
			finally
			{
				if (hasOpened)
					CloseConnection();
			}
		}

		/// <summary>
		/// Executes a script of sql commands separated by semi-colons
		/// </summary>
		/// <param name="script">Set of Sql commands separated by semicolons</param>
		public async Task ExecScriptAsync(string script)
		{
			var segments = script.Split(';');
			var command = new SqlCommand()
			{
				Connection = _connection,
				CommandType = CommandType.Text,
				CommandTimeout = _timeout, /*secs*/
				Transaction = _transaction
			};

			await OpenConnectionAsync().ConfigureAwait(false);

			try
			{
				foreach (var segment in segments)
				{
					if (string.IsNullOrWhiteSpace(segment))
						continue;
					command.CommandText = segment;
					await command.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
			}
			finally
			{
				CloseConnection();
			}
		}

		#region Transaction Methods

		/// <summary>
		/// Begins new transaction
		/// </summary>
		public void BeginTransaction()
        {
            if (_transaction != null)
                throw new Exception("Transaction already exists");
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

		/// <summary>
		/// Begins new transaction
		/// </summary>
		public void BeginTransaction(IsolationLevel iso)
		{
			if (_transaction != null)
				throw new Exception("Transaction already exists");
			_connection.Open();
			_transaction = _connection.BeginTransaction(iso);
		}

		
		/// <summary>
		/// Begins new transaction, asynchronously
		/// </summary>
		/// <returns></returns>
		public async Task BeginTransactionAsync()
		{
			if (_transaction != null)
				throw new Exception("Transaction already exists");
			await _connection.OpenAsync().ConfigureAwait(false);
			_transaction = _connection.BeginTransaction();
		}


		/// <summary>
		/// Begins new transaction, asynchronously
		/// </summary>
		/// <returns></returns>
		public async Task BeginTransactionAsync(IsolationLevel iso)
		{
			if (_transaction != null)
				throw new Exception("Transaction already exists");
			await _connection.OpenAsync().ConfigureAwait(false);
			_transaction = _connection.BeginTransaction(iso);
		}


		/// <summary>
		/// Rollbacks all changes since beginning of transaction...destroys transaction
		/// </summary>
		public void RollbackTransaction()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
            _connection.Close();
        }

        /// <summary>
        /// Commits and then destroys transaction
        /// </summary>
        public void CommitTransaction()
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
            _connection.Close();
        }

		#endregion

		/// <summary>
		/// Returns information schema for database
		/// </summary>
		public InformationSchema GetSchema()
		{
			return new InformationSchema(this);
		}

		/// <summary>
		/// INSERT INTO table(a,b,c) OUTPUT inserted.id VALUES(@0,@1,...)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Func<SqlDb, T, bool> CreateAddMethod<T>()
		{
			var entityType = typeof(T);
			var tableAttr = entityType.GetCustomAttribute<TableAttribute>(false);
			string tableName;

			bool optIn;
			if (tableAttr == null)
			{
				tableName = entityType.Name;
				optIn = false;
			}
			else
			{
				tableName = tableAttr.TableName;
				optIn = tableAttr.OptIn;
			}

			var prefix = new StringBuilder();
			prefix.AppendFormat("INSERT INTO \"{0}\" (", tableName);

			var suffix = new StringBuilder(" VALUES(");

			string outputClause = string.Empty;
			var inputArgs = new List<PropertyInfo>();
			PropertyInfo output = null;

			if (optIn)
			{
				ColumnAttribute columnAttr;
				foreach (var propInfo in entityType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (propInfo.TryGetAttribute<ColumnAttribute>(true, out columnAttr))
						continue;

					var columnName = columnAttr.ColumnName ?? propInfo.Name;

					if (columnAttr.IsPrimaryKey)
					{
						outputClause = string.Format(" OUTPUT inserted.\"{0}\"", columnName);
						output = propInfo;
					}
					else
					{
						prefix.AppendFormat("\"{0}\",", columnName);
						suffix.AppendFormat("@{0},", inputArgs.Count);
						inputArgs.Add(propInfo);
					}
				}
			}
			else
			{
				ColumnAttribute columnAttr;
				foreach (var propInfo in entityType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string columnName;
					if (propInfo.TryGetAttribute<ColumnAttribute>(true, out columnAttr))
					{
						columnName = columnAttr.ColumnName ?? propInfo.Name;
						if (columnAttr.IsPrimaryKey)
						{
							outputClause = string.Format(" OUTPUT inserted.\"{0}\"", columnName);
							output = propInfo;
							continue;
						}
					}
					else
						columnName = propInfo.Name;

					prefix.AppendFormat("\"{0}\",", columnName);
					suffix.AppendFormat("@{0},", inputArgs.Count);
					inputArgs.Add(propInfo);
				}
			}

			var dbType = typeof(SqlDb);

			prefix.Length--;
			prefix.Append(')');
			prefix.Append(outputClause);
			suffix.Length--;
			suffix.Append(')');
			prefix.Append(suffix.ToString());


			var query = prefix.ToString();
			var dbParam = Expression.Parameter(typeof(SqlDb));
			var entityParam = Expression.Parameter(entityType);
			var commandP = Expression.Parameter(typeof(SqlCommand));

			var parametersP = Expression.Parameter(typeof(DbParameterCollection));


			var statements = new List<Expression>();
			statements.Add(
				Expression.Assign(commandP,                  // command = CreateQuery(query);
					Expression.Call(dbParam, dbType.GetMethod("CreateQuery", new Type[] { typeof(string) }),
						Expression.Constant(query))));
			//parametersP = command.Parameters

			statements.Add(Expression.Assign(parametersP,
				Expression.Property(commandP,
				commandP.Type.GetProperty("Parameters", parametersP.Type))));

			var addMethod = typeof(DbParameterCollection).GetMethod("Add", new Type[] { typeof(object) });

			for (int i = 0; i < inputArgs.Count; i++)
			{
				var sqlParam = ExprBuilder.ToParameterExpr(string.Format("@{0}", i), Expression.Property(entityParam, inputArgs[i]));
				statements.Add(Expression.Call(parametersP, addMethod, sqlParam));
			}

			var label = Expression.Label(typeof(bool));
			Expression queryMethod;
			if (output != null)
			{
				// public T Scalar<T>(IDbCommand command, T defaultValue)
				var scalarMethod = dbType.FindGenericMethod("Scalar", typeof(IDbCommand), output.PropertyType)
					.MakeGenericMethod(output.PropertyType);

				Expression defValue = Expression.Constant(-1, output.PropertyType);
				queryMethod = Expression.Return(label,
					Expression.NotEqual(
						Expression.Assign(
							Expression.Property(entityParam, output),
							Expression.Call(dbParam, scalarMethod,
								commandP,
								defValue)),
						defValue));
			}
			else
			{
				queryMethod = Expression.Return(label,
					Expression.NotEqual(
						Expression.Call(dbParam,
							dbType.GetMethod("NonQuery", new Type[] { typeof(IDbCommand) }),
							commandP),
						Expression.Constant(0)));
			}
			statements.Add(queryMethod);
			statements.Add(Expression.Label(label, Expression.Constant(false)));

			var block = Expression.Block(typeof(bool),
				new ParameterExpression[] { commandP, parametersP },
				statements);

			return Expression.Lambda<Func<SqlDb, T, bool>>(block, dbParam, entityParam).Compile();
		}

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _connection = new SqlConnection(_connection.ConnectionString);
        }

		/// <summary>
		/// Converts name value pair into a sql command parameter
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static SqlParameter ToParameter(string name, object value)
        {
            SqlParameter sqlParameter;

            if (value == null)
            {
                sqlParameter = new SqlParameter(name, DBNull.Value);
            }
            else if (value is string)
            {
                sqlParameter = new SqlParameter(name, value);
            }
            else if (value is XmlReader)
            {
                sqlParameter = new SqlParameter(name, SqlDbType.Xml)
                {
                    Value = value
                };
            }
			else if (value is byte[])
			{
				sqlParameter = new SqlParameter(name, value);
			}
            else if (value is System.Collections.IEnumerable)
            {
                sqlParameter = new SqlParameter(name, value)
                {
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.Structured
                };
            }
            else if (value.GetType().IsEnum)
            {
                sqlParameter = new SqlParameter(name, (int)value);
            }
            else
            {
                var type = value.GetType();
                //if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                //{
                //    value = type.GetProperty("Value").GetValue(value, null);
                //}
                if (type == typeof(uint))
                {
                    value = (int)((uint)value);
                }
                else if (type == typeof(ushort))
                {
                    value = (short)((ushort)value);
                }
                sqlParameter = new SqlParameter(name, value);
            }
            return sqlParameter;
        }
    }
	
    /// <summary>
	/// Table attribute
	/// </summary>
	public class TableAttribute: Attribute
    {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="tableName">Table name</param>
		public TableAttribute(string tableName)
        {
            _tableName = tableName;
        }

		#region Properties

		#region TableName Property
		/// <summary>
		/// Table name property
		/// </summary>
		public string TableName 
        {
            get { return _tableName; }
            set { _tableName = value; }
        }
        private string _tableName;
        #endregion

        #region OptIn Property
        /// <summary>
        /// If true, will only scan properties with column attribute
        /// </summary>
        public bool OptIn { get; set; }
        #endregion

        #endregion
    }

	/// <summary>
	/// Type extension methods
	/// </summary>
    public static class TypeExt
    {
		/// <summary>
		/// Finds method with parameter or generic parameters matching args
		/// </summary>
		/// <param name="type">Type</param>
		/// <param name="name">Method name</param>
		/// <param name="args">(Desired) Method signature</param>
		/// <returns>Matching method, if found</returns>
		public static MethodInfo FindGenericMethod(this Type type, string name, params Type[] args)
        {
            foreach (var methodInfo in type.GetMethods())
            {
                if (methodInfo.Name == name)
                {
                    var methodParams = methodInfo.GetParameters();
                    if (methodParams.Length == args.Length)
                    {
                        for (int i = 0; ; i++)
                        {
                            if (i >= methodParams.Length)
                                return methodInfo;
                            else if (!methodParams[i].ParameterType.CompareGeneric(args[i]))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return null;
        }

		/// <summary>
		/// Returns true if parameter type matches or is generic
		/// </summary>
		public static bool CompareGeneric(this Type paramType, Type type)
        {
            bool isOk;
            if (paramType == type)
            {
                isOk = true;
            }
            else if (paramType.IsGenericParameter)
            {
                isOk = true;
            }
            else
            {
                isOk = false;
            }
            return isOk;
        }
    }
}
