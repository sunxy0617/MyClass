using Newtonsoft.Json.Linq;
using StudentManagement_Server.MyClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using HdCodeLibrary;

namespace StudentManagement_Server.Controllers
{
    public class WebapiController : MyGetController
    {
        // GET: Webapi
        public string Index()
        {
            return "欢迎使用";
        }

        #region 学校管理

        /// <summary>
        /// 添加学校
        /// </summary>
        /// <param name="name">机构名</param>
        /// <param name="pwd">密码</param>
        /// <param name="address">地址</param>
        /// <param name="phone">手机号</param>
        /// <param name="logo">logo（image src）</param>
        /// <param name="images">场景图（image src json数组）</param>
        /// <param name="businessLicense">营业执照（image src）</param>
        /// <returns>学校ID</returns>
        [AllowCrossSiteJson]
        public JsonResult AddSchool(string name, string pwd, string address, string phone, string logo, string images, string businessLicense)
        {
            if (pwd == null)
                return Json(new { state = false, msg = "Password needs to be encrypted with MD5" });

            try
            {
                var jArr = JArray.Parse(images);
                for (int i = 0; i < jArr.Count; i++)
                {
                    var json = jArr[i];
                    if (string.IsNullOrEmpty(json.ToString()))
                    {
                        jArr.RemoveAt(i);
                        i--;
                    }
                }
                var imageCount = jArr.Count;
                var schoolId = Database.AddSchool(pwd, name, address, phone, imageCount);
                var schoolPath = Server.MapPath("/Resources/school/" + schoolId);
                var schoolScenesPath = schoolPath + "/scenes";

                if (!Directory.Exists(schoolScenesPath))
                {
                    _ = Directory.CreateDirectory(schoolScenesPath);
                }

                SaveAndCompressImage(logo, schoolPath, "logo");
                SaveAndCompressImage(businessLicense, schoolPath, "businessLicense");

                for (int i = 0; i < imageCount; i++)
                {
                    var json = jArr[i];
                    var imgBase64 = json.ToString();
                    SaveAndCompressImage(imgBase64, schoolScenesPath, "scene" + i);
                }


                return Json(new { state = true, schoolId = schoolId });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { state = false });
            }
        }
        [AllowCrossSiteJson]
        public JsonResult SchoolLoginById(string id, string pwd)
        {
            if (id == "test_user")//测试用户
            {
                return Json(new { state = true, uid = "test_user", token = "test_user" });
            }
            var loginToken = Database.SchoolLoginById(id, pwd);
            if (string.IsNullOrEmpty(loginToken) || loginToken == "fail")
                return Json(new { state = false });
            else
            {
                return Json(new { state = true, id = id, token = loginToken });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public JsonResult SchoolLoginByToken(string id, string token)
        {
            if (id == "test_user")//测试用户
            {
                return Json(new { state = true, isNewUser = false, uid = "test_user", token = "test_user" });
            }
            var loginToken = Database.SchoolLoginByToken(id, token);
            if (string.IsNullOrEmpty(token) || token == "fail")
                return Json(new { state = false });
            else
            {
                return Json(new { state = true, id = id, token = loginToken });
            }
        }
        [AllowCrossSiteJson]
        public JsonResult GetSchoolInfo(string id)
        {
            using (var table = Database.GetSchoolInfo(id))
            {
                if (table == null || table.Rows.Count < 1)
                {
                    return Json(new { state = false });
                }
                else
                {
                    table.Columns.Add("state", typeof(bool));

                    var row = table.Rows[0];
                    row["state"] = true;

                    var dic = RowToDictionary(row);

                    return Json(dic);
                }
            }
        }

        [AllowCrossSiteJson]
        public JsonResult GetAllSchool(int? startId, int? endId)
        {
            using (var table = Database.GetAllSchool(startId, endId))
            {
                if (table == null || table.Rows.Count == 0)
                    return Json(new { state = false });
                var schoolList = TableToList(table);
                return Json(new { state = true, schoolList });
            }
        }

        #endregion



        #region 班级添加

        /// <summary>
        /// 添加班机
        /// </summary>
        /// <param name="schoolId">学校Id</param>
        /// <param name="token">学校token</param>
        /// <param name="name">班级名称</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public JsonResult AddClass(string schoolId, string token, string name, string pwd)
        {
            var result = Database.AddClass(schoolId, token, name, pwd);
            if (result == null)
            {
                return Json(new { state = false });
            }

            return Json(new { state = true, schoolId = schoolId, classId = result[0] });
        }
        /// <summary>
        /// 使用班级密码登录
        /// </summary>
        /// <param name="id">班级Id</param>
        /// <param name="schoolId">学校Id</param>
        /// <param name="pwd">班级密码</param>
        /// <returns>登录状态数组</returns>
        [AllowCrossSiteJson]
        public JsonResult ClassLoginById(string id, string schoolId, string pwd)
        {
            if (id == "test_user")//测试用户
            {
                return Json(new { state = true, uid = "test_user", token = "test_user" });
            }
            var loginToken = Database.ClassLoginById(id, schoolId, pwd);
            if (string.IsNullOrEmpty(loginToken) || loginToken == "fail")
                return Json(new { state = false });
            else
            {
                return Json(new { state = true, id = id, token = loginToken });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public JsonResult ClassLoginByToken(string id, string schoolId, string token)
        {
            if (id == "test_user")//测试用户
            {
                return Json(new { state = true, isNewUser = false, uid = "test_user", token = "test_user" });
            }
            var loginToken = Database.ClassLoginByToken(id, schoolId, token);
            if (string.IsNullOrEmpty(token) || token == "fail")
                return Json(new { state = false });
            else
            {
                return Json(new { state = true, id = id, token = loginToken });
            }
        }
        [AllowCrossSiteJson]
        public JsonResult GetClassInfo(string id, string schoolId, string token)
        {
            using (var table = Database.GetClassInfo(id, schoolId, token))
            {
                if (table == null || table.Rows.Count < 1)
                {
                    return Json(new { state = false });
                }
                else
                {
                    table.Columns.Add("state", typeof(bool));

                    var row = table.Rows[0];
                    row["state"] = true;

                    var dic = RowToDictionary(row);

                    return Json(dic);
                }
            }
        }
        /// <summary>
        /// 获取学校里所有班级
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public JsonResult GetAllClass(string id)//此函数和GetAllClassInSchool()功能完全相同，为了名称统一标准化添加，建议平时使用本函数
        {
            using (var table = Database.GetAllClass(id))
            {
                if (table == null || table.Rows.Count == 0)
                    return Json(new { state = false });
                var schoolList = TableToList(table);
                return Json(new { state = true, schoolList });
            }
        }
        /// <summary>
        /// 获取学校里所有班级
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowCrossSiteJson, Obsolete]
        public JsonResult GetAllClassInSchool(string id)
        {
            using (var table = Database.GetAllClass(id))
            {
                if (table == null || table.Rows.Count == 0)
                    return Json(new { state = false });
                var schoolList = TableToList(table);
                return Json(new { state = true, schoolList });
            }
        }
        #endregion

        #region 学生管理
        /// <summary>
        /// 添加单个学生
        /// </summary>
        /// <param name="schoolId">学校Id</param>
        /// <param name="classId">班级Id</param>
        /// <param name="idCard">身份证号</param>
        /// <param name="pwd">密码</param>
        /// <param name="name">姓名</param>
        /// <param name="sex">性别true为男，false为女</param>
        /// <param name="headImg">头像base64</param>
        /// <param name="sno">学号</param>
        /// <returns>添加单个学生</returns>
        [AllowCrossSiteJson]
        public JsonResult AddStudent(string schoolId, string classId, string idCard, string pwd, string name, bool sex, string headImg, string sno)
        {
            if (string.IsNullOrEmpty(pwd))
                return Json(new { state = false, msg = "Password cannot null" });
            var hdcode = new Hdcode();
            var rule = BitmapTools.Name2Rule(name, out var size);
            var planNum = hdcode.addPlan("学生：" + name, rule, size);
            var studentId = Database.AddStudent(schoolId, classId, idCard, pwd, name, sex, sno, planNum);
            if (studentId == -1)
                return Json(new { state = false, studentId, msg = "插入失败" });
            else if (studentId == -2)
                return Json(new { state = false, studentId, msg = "身份证号已存在" });
            else
            {
                var studentPath = Server.MapPath("/Resources/student/" + studentId);
                if (!Directory.Exists(studentPath))
                {
                    _ = Directory.CreateDirectory(studentPath);
                }
                SaveAndCompressImage(headImg, studentPath, "headImg");

                return Json(new { state = true, studentId });
            }
        }
        /// <summary>
        /// 学生登录
        /// </summary>
        /// <param name="id">学生id或身份证号</param>
        /// <param name="pwd"></param>
        /// <returns>token</returns>
        [AllowCrossSiteJson]
        public JsonResult StudentLoginById(string id, string pwd)
        {
            if (id == "test_user")//测试用户
            {
                return Json(new { state = true, uid = "test_user", token = "test_user" });
            }
            var loginToken = Database.StudentLoginById(ref id, pwd);
            if (string.IsNullOrEmpty(loginToken) || loginToken == "fail")
                return Json(new { state = false });
            else
            {
                return Json(new { state = true, id = id, token = loginToken });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public JsonResult StudentLoginByToken(string id, string token)
        {
            if (id == "test_user")//测试用户
            {
                return Json(new { state = true, isNewUser = false, uid = "test_user", token = "test_user" });
            }
            var loginToken = Database.StudentLoginByToken(id, token);
            if (string.IsNullOrEmpty(token) || token == "fail")
                return Json(new { state = false });
            else
            {
                return Json(new { state = true, id = id, token = loginToken });
            }
        }
        /// <summary>
        /// 获取学生信息，
        /// </summary>
        /// <param name="id">必填项，学生id</param>
        /// <param name="token">选填项，学生token，不填写时只返回基本公开信息</param>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public JsonResult GetStudentInfo(string id, string token)
        {
            using (var table = Database.GetStudentInfo(id, token))
            {
                if (table == null || table.Rows.Count < 1)
                {
                    return Json(new { state = false });
                }
                else
                {
                    table.Columns.Add("state", typeof(bool));

                    var row = table.Rows[0];
                    row["state"] = true;

                    var dic = RowToDictionary(row);

                    return Json(dic);
                }
            }
        }
        /// <summary>
        /// 获取所有在校或班级学生
        /// </summary>
        /// <param name="schoolId">学校Id</param>
        /// <param name="classId">班级id，为空时获取整个学校学生</param>
        /// <returns></returns>
        [AllowCrossSiteJson]
        public JsonResult GetAllStudent(string schoolId, string classId)
        {
            using (var table = Database.GetAllStudent(schoolId,classId))
            {
                if (table == null)
                    return Json(new { state = false });
                var healthTable = TableToList(table);
                return Json(new { state = true, healthTable });
            }
        }
        #endregion

        #region 学生健康管理
        /// <summary>
        /// 添加学生体温
        /// </summary>
        /// <param name="schoolId">学校Id</param>
        /// <param name="classId">检测班级Id</param>
        /// <param name="studentId">被检测学生Id</param>
        /// <param name="classToken">操作员班级token</param>
        /// <param name="temperature">体温</param>
        /// <returns>数据库主键编号</returns>
        [AllowCrossSiteJson]
        public JsonResult AddStudentTemperature(string schoolId, string classId, string studentId, string classToken, float temperature)
        {
            var key = Database.AddStudentTemperature(schoolId, classId, studentId, classToken, temperature);
            if (key == -1)
                return Json(new { state = false, err_code = key, msg = "token和班级不匹配" });
            else if (key == -2)
                return Json(new { state = false, err_code = key, msg = "学生不存在" });
            else if (key == -3)
                return Json(new { state = false, err_code = key, msg = "不是本校学生" });
            else if (key == -4)
                return Json(new { state = false, err_code = key, msg = "数据类型有误，请检查重试" });
            else
                return Json(new { state = true, healthCode = key });

        }
        
        [AllowCrossSiteJson]
        public JsonResult GetStudentTemperatureByDate(string studentId, string startTime, string endTime)
        {
            using (var table = Database.GetStudentTemperatureByDate(studentId,startTime,endTime))
            {
                if (table == null)
                    return Json(new { state = false });
                var healthTable = TableToList(table);
                return Json(new { state = true, healthTable });
            }
        }
        #endregion

        #region 私有处理工具
        private void SaveAndCompressImage(string base64, string path, string fileName)
        {
            var imgPath = path + "/" + fileName + ".jpg";
            var imgMiniPath = path + "/" + fileName + ".min.jpg";
            var bmp = BitmapTools.Base64ToBitmap(base64);
            if (bmp == null)
                return;

            var bmpMini = BitmapTools.MakeMiniBitmap(bmp, 200);
            bmp.Save(imgPath, ImageFormat.Jpeg);
            bmpMini.Save(imgMiniPath, ImageFormat.Jpeg);
            bmp.Dispose();
            bmpMini.Dispose();
        }

        public List<Dictionary<string, object>> TableToList(DataTable dtData)//DataTable转数组，用于后续转JsonResult
        {
            List<Dictionary<string, object>>
                lstRows = new List<Dictionary<string, object>>();

            foreach (DataRow dr in dtData.Rows)
            {
                var dictRow = new Dictionary<string, object>();
                foreach (DataColumn col in dtData.Columns)
                {
                    dictRow.Add(col.ColumnName, dr[col]);
                }
                lstRows.Add(dictRow);
            }
            return lstRows;
        }

        public Dictionary<string, object> RowToDictionary(DataRow dtData)//DataRow转键值，用于后续转JsonResult
        {
            var dictRow = new Dictionary<string, object>();
            foreach (DataColumn col in dtData.Table.Columns)
            {
                dictRow.Add(col.ColumnName, dtData[col]);
            }

            return dictRow;
        }

        #endregion

    }
}