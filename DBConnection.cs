using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BTL
{
    /// <summary>
    /// Lớp dùng chung cho việc sử lý đến DataBase
    /// </summary>
    public class DBConnection
    {
        /// <summary>
        /// Làm cho màu mè chuẩn tính đóng gói của OOP =)))
        /// <br/>
        /// Là <see cref="System.Data.DataSet"/> chứa các dữ liệu của <see cref="DataTable"/> khi sử dụng các hàm select, update, ...
        /// </summary>
        public DataSet DataSet => _dataSet;


        /// <summary>
        /// Làm cho màu mè chuẩn tính đóng gói của OOP =))) 
        /// <br/>
        /// Là 1 thể hiện hay là gì đấy =)) k biết dịch là gì
        /// <br/>
        /// Chủ yếu là để sử dụng các hàm của lớp không cần phải tạo ra đối tượng mới từ lớp
        /// </summary>
        public static DBConnection Instance => _instance;

        /// <summary>
        /// Làm cho màu mè chuẩn tính đóng gói của OOP =)))
        /// </summary>
        private static DBConnection _instance = new DBConnection();

        /// <summary>
        /// Làm cho màu mè chuẩn tính đóng gói của OOP =)))
        /// </summary>
        private DataSet _dataSet = new DataSet();

        /// <summary>
        /// Hàm tạo <see cref="SqlConnection"/> từ <see cref="ConfigurationManager"/> ConnectionString "dbConnection"
        /// </summary>
        /// <returns><see cref="SqlConnection"/></returns>
        public SqlConnection CreateConnection()
        {
            string constr = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
            return new SqlConnection(constr);
        }

        /// <summary>
        /// Hàm tạo <see cref="SqlParameter"/> đầy đủ để chuẩn theo các hàm sử lý như insert, update
        /// </summary>
        /// <param name="parameterName">Tên tham số, thường bắt đàu bằng @ VD: @sHoTen</param>
        /// <param name="dbType">Loại thuộc tính VD: <see cref="SqlDbType.NChar"/> là nvarchar trong database,<see cref="SqlDbType.bit"/> là bit trong database, ... </param>
        /// <param name="size">Kích thước của dữ liệu</param>
        /// <param name="sourceColumn">Tên cột trong DataBase VD: sHoTen</param>
        /// <param name="value">Giá trị cho cột</param>
        /// <returns><see cref="SqlParameter"/></returns>
        public SqlParameter BuildParameter( string parameterName, SqlDbType dbType, int size, string sourceColumn, object value)
        {
            return new SqlParameter(parameterName, dbType, size, sourceColumn) { Value = value };
        }


        /// <summary>
        /// Hàm lấy dữ liệu từ DataBase
        /// <br/>
        /// Nó sẽ tự cập nhật thêm vào <see cref="DataSet"/> nên tất cả mọi thứ binding đến sẽ update theo
        /// </summary>
        /// <param name="table">Tên bảng</param>
        /// <param name="query">(Nếu có) Mã SQL sau WHERE VD: "bDeleted=0 AND sHoTen LIKE %N%"</param>
        /// <returns>Dữ liệu của bảng dạng DataTable</returns>
        public DataTable SelectDB(string table, string query = "")
        {
            using (SqlConnection cnn = CreateConnection())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (SqlCommand cmd = cnn.BuildSelectCommand(table, query))
                    {
                        try
                        {
                            da.SelectCommand = cmd;
                            _dataSet.Tables[table]?.Clear();
                            cnn.Open();
                            da.Fill(_dataSet, table);
                            cnn.Close();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Lỗi select DataBase : " + ex.Message);
                        }
                    }
                }
            }
            return _dataSet.Tables[table];
        }

        /// <summary>
        /// Hàm thêm dữ liệu bằng Command vào DataBase
        /// <br/>
        /// Nó sẽ tự cập nhật thêm vào <see cref="DataSet"/> nên tất cả mọi thứ binding đến sẽ update theo
        /// </summary>
        /// <param name="table">Tên bảng</param>
        /// <param name="sqlParameters">Các thuộc tính cần thêm vào bảng (sử dụng <see cref="BuildParameter"/> để tạo ra parameter cho chuẩn dạng dữ liệu)</param>
        /// <returns><c>True</c> Nếu thêm thành công, ngược lại <c>False</c></returns>
        public bool InsertDB(string table, params SqlParameter[] sqlParameters)
        {
            using (SqlConnection cnn = CreateConnection())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (SqlCommand cmd = cnn.BuildInsertCommand(table, sqlParameters))
                    {
                        try
                        {
                            da.InsertCommand = cmd;
                            DataRow newRow = _dataSet.Tables[table].NewRow();
                            foreach (var p in sqlParameters)
                            {
                                newRow[p.SourceColumn] = p.Value;
                            }
                            _dataSet.Tables[table].Rows.Add(newRow);
                            cnn.Open();
                            int i = da.Update(_dataSet, table);
                            cnn.Close();
                            return i > 0;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Lỗi insert DataBase : " + ex.Message);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Hàm thêm dữ liệu bằng Proc vào DataBase
        /// <br/>
        /// Nó sẽ tự cập nhật thêm vào <see cref="DataSet"/> nên tất cả mọi thứ binding đến sẽ update theo
        /// </summary>
        /// <param name="table">Tên bảng</param>
        /// <param name="nameProc">Tên hàm sử dụng trong để thêm dữ liệu</param>
        /// <param name="sqlParameters">Các thuộc tính cần thêm vào bảng (sử dụng <see cref="BuildParameter)"/> để tạo ra parameter cho chuẩn dạng dữ liệu)</param>
        /// <returns><c>True</c> Nếu thêm thành công, ngược lại <c>False</c></returns>
        public bool InsertDB(string table, string nameProc,params SqlParameter[] sqlParameters)
        {
            if (_dataSet.Tables[table] == null) SelectDB(table);

            using (SqlConnection cnn = CreateConnection())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (SqlCommand cmd = cnn.BuildInsertProc(nameProc, sqlParameters))
                    {
                        try
                        {
                            da.InsertCommand = cmd;
                            DataRow newRow = _dataSet.Tables[table].NewRow();
                            foreach (var p in sqlParameters)
                            {
                                newRow[p.SourceColumn] = p.Value;
                            }
                            _dataSet.Tables[table].Rows.Add(newRow);
                            cnn.Open();
                            int i = da.Update(_dataSet, table);
                            cnn.Close();
                            return i > 0;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Lỗi insert DataBase : " + ex.Message);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Hàm lấy dữ liệu của 1 bản ghi bất khì nào trong bảng
        /// </summary>
        /// <param name="table">Tên bảng</param>
        /// <param name="sourceColumn">Tên cột để kiểm tra lấy ra bản ghi (Nên sử dụng ID)</param>
        /// <param name="value">Giá trị của cột để kiểm tra lấy ra bản ghi (Nên sử dụng ID)</param>
        /// <returns><c><see cref="DataRow"/></c>Nếu thì thấy bản ghi phù hợp với <paramref name="value"/>, ngược lại <see cref="null"/></returns>
        public DataRow GetRow(string table, string sourceColumn, object value)
        {
            if (_dataSet.Tables[table] == null) SelectDB(table);

            foreach (DataRow row in _dataSet.Tables[table].Rows)
            {
                if (row[sourceColumn].Equals(value))
                    return row;
            }
            return null;
        }


        /// <summary>
        /// Hàm cập nhật dữ liệu bằng Command vào DataBase
        /// <br/>
        /// Nó sẽ tự cập nhật thêm vào <see cref="DataSet"/> nên tất cả mọi thứ binding đến sẽ update theo
        /// </summary>
        /// <param name="table">Tên bảng</param>
        /// <param name="condition">Tên điều kiện cập nhật (sử dụng <see cref="BuildParameter)"/> để tạo ra parameter cho chuẩn dạng dữ liệu)</param>
        /// <param name="sqlParameters">Các thuộc tính cần cập nhật vào bảng (sử dụng <see cref="BuildParameter)"/> để tạo ra parameter cho chuẩn dạng dữ liệu)</param>
        /// <returns><c>True</c> Nếu cập nhật thành công, ngược lại <c>False</c></returns>
        public bool UpdateDB(string table, SqlParameter condition, params SqlParameter[] sqlParameters)
        {
            DataTable dt = _dataSet.Tables[table];
            if (_dataSet.Tables[table] == null)
            {
                dt = SelectDB(table);
            }

            using (SqlConnection cnn = CreateConnection())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    using (SqlCommand cmd = cnn.BuildUpdateCommand(table, condition, sqlParameters))
                    {
                        try
                        {
                            da.UpdateCommand = cmd;
                            cnn.Open();

                            var row = GetRow(table, condition.SourceColumn, condition.Value);

                            foreach (var p in sqlParameters)
                            {
                                row[p.SourceColumn] = p.Value;
                            }
                            int i = da.Update(_dataSet, table);
                            cnn.Close();
                            return i > 0;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Lỗi update DataBase : " + ex.Message);
                        }
                    }

                }
            }
        }
    }
}
