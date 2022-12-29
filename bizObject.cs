﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data.SqlClient;
using System.Data;
using Dapper;

namespace CPUFramework
{
    public class bizObject<T> : INotifyPropertyChanged where T : bizObject<T>, new()
    {
        protected enum SprocActionEnum { Get, Update, Delete }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bizObject()
        {

        }

        public static T Get(string paramname,object value)
        {
            T tobj;
            
            DynamicParameters dp = new DynamicParameters();
            dp.Add(paramname, value);
            tobj = SQLUtility.ExecuteGetSingleDapper<T>(SprocName(SprocActionEnum.Get), dp);
            
            return tobj;
        }

        public static T Get(int Id)
        {
            T tobj;
            if (Id == 0)
            {
                tobj = new();
            }
            else
            {
                tobj = Get(PrimaryKeyName, Id);
            }
            return tobj;
        }

        public static List<T> GetList(string paramname, object value)
        {
            DynamicParameters dp = new DynamicParameters();
            dp.Add(paramname, value);
            return SQLUtility.ExecuteGetListDapper<T>(SprocName(SprocActionEnum.Get), dp);
        }

        public static List<T> GetAll()
        {
            return GetList("All", true);
        }
        public void Delete()
        {
            DynamicParameters dp = new DynamicParameters();
            dp.Add(PrimaryKeyName, this.PrimaryKeyValue);
            SQLUtility.ExecuteSQLDapper(SprocName(SprocActionEnum.Delete), dp);


        }

        public void Save()
        {
            DynamicParameters dp = new DynamicParameters(this);
           
            dp.Add(PrimaryKeyName, this.PrimaryKeyValue, DbType.Int32, ParameterDirection.InputOutput);

            this.DynamParamsForUpdate.AddDynamicParams(dp);

            SQLUtility.ExecuteSQLDapper(SprocName(SprocActionEnum.Update), this.DynamParamsForUpdate);

            this.PrimaryKeyValue = this.DynamParamsForUpdate.Get<int>(PrimaryKeyName);
        }

        public static List<T> GetAllFromSproc(string sprocname, DynamicParameters dynaparam)
        {
            return SQLUtility.ExecuteGetListDapper<T>(sprocname, dynaparam);
        }

        public void InvokePropertyChanged([CallerMemberName] string propertname = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertname));
        }

        protected static string TableName
        {
            get
            {
                return typeof(T).ToString().Split(".")[1].Replace("biz", "");
            }
        }


        protected int PrimaryKeyValue { get; set; }
        protected static string PrimaryKeyName { get => TableName + "Id"; }
        protected static string SprocName(SprocActionEnum sprocaction)
        {
            return TableName + sprocaction.ToString();
        }

        protected DynamicParameters DynamParamsForUpdate { get; } = new();



    }
}
