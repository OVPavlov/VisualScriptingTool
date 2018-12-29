using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

public class ReflectedValue
{
    static readonly object[] EmptyArray = new object[0];
    public Type Type;
    object _obj;
    FieldInfo[] _fields;
    PropertyInfo[] _properties;

    public object GetValue()
    {
        object obj = _obj;
        for (int i = 0; i < _fields.Length; i++)
        {
            if (_fields[i] != null)
                obj = _fields[i].GetValue(obj);
            else
                obj = _properties[i].GetValue(obj, EmptyArray);
        }
        return obj;
    }
    public void SetValue(object val)
    {
        object obj = _obj;
        int lastElement = _fields.Length - 1;
        for (int i = 0; i < lastElement; i++)
        {
            if (_fields[i] != null)
                obj = _fields[i].GetValue(obj);
            else
                obj = _properties[i].GetValue(obj, EmptyArray);
        }
        if (_fields[lastElement] != null)
            _fields[lastElement].SetValue(obj, val);
        else
            _properties[lastElement].SetValue(obj, val, EmptyArray);
    }

    public bool FindMember(object objectIn, string address)
    {
        if (address == null) return false;
        string[] fields = address.Split('.');
        Type type;

        int shift = 0;
        if (objectIn != null)
        {
            type = objectIn.GetType();
        }
        else
        {
            shift = 1;
            type = Type.GetType(fields[0]);
            if (type == null)
            {
                type = typeof(Time).Assembly.GetType("UnityEngine." + fields[0]);
                if (type == null) return false;
            }
        }
        _obj = objectIn;


        bool found = FindMemberPath(type, fields, out _fields, out _properties, shift);
        if (found)
        {
            if (_fields[_fields.Length - 1] != null)
                Type = _fields[_fields.Length - 1].FieldType;
            else
                Type = _properties[_properties.Length - 1].PropertyType;
        }

        return found;
    }
    static bool FindMemberPath(Type type, string[] fields, out FieldInfo[] fieldInfos, out PropertyInfo[] propertyInfos, int shift)
    {
        fieldInfos = null;
        propertyInfos = null;

        if (fields.Length < (1 + shift)) return false;
        if (type == null) return false;

        fieldInfos = new FieldInfo[fields.Length - shift];
        propertyInfos = new PropertyInfo[fields.Length - shift];

        for (int i = shift; i < fields.Length; i++)
        {
            string fieldName = fields[i];
            FieldInfo fi = type.GetField(fieldName);
            if (fi != null)
            {
                fieldInfos[i - shift] = fi;
                type = fi.FieldType;
            }
            else
            {
                PropertyInfo pi = type.GetProperty(fieldName);
                if (pi == null)
                {
                    fieldInfos = null;
                    propertyInfos = null;
                    return false;
                }
                propertyInfos[i - shift] = pi;
                type = pi.PropertyType;
            }
        }
        return true;
    }

    public bool ValidateGet()
    {
        for (int i = 0; i < _fields.Length; i++)
        {
            if (_fields[i] == null)
                if (_properties[i].GetGetMethod() == null)
                    return false;
        }
        return true;
    }
    public bool ValidateSet()
    {
        for (int i = 0; i < _fields.Length; i++)
        {
            if (_fields[i] == null)
                if (_properties[i].GetSetMethod() == null)
                    return false;
        }
        return true;
    }

    public Func<T> CompileGetMethod<T>()
    {
        Expression exp = null;
        if (_obj != null)
        {
            FieldInfo fi = GetType().GetField("_obj", BindingFlags.NonPublic | BindingFlags.Instance);
            exp = Expression.Field(Expression.Constant(this), fi);
            exp = Expression.Convert(exp, _obj.GetType());
        }

        for (int i = 0; i < _fields.Length; i++)
        {
            if (_fields[i] != null)
                exp = Expression.Field(exp, _fields[i]);
            else
                exp = Expression.Property(exp, _properties[i]);
        }

        Expression<Func<T>> expression = Expression.Lambda<Func<T>>(exp, new ParameterExpression[0]);
        return expression.Compile();
    }

    public Action<T> CompileSetMethod<T>()
    {
        bool isStatic = _obj == null;
        Type thisType = typeof(ReflectedValue);

        DynamicMethod dynMethod = new DynamicMethod("", typeof(void), new Type[] { thisType, typeof(T) }, thisType);
        ILGenerator il = dynMethod.GetILGenerator();

        if (!isStatic)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, thisType.GetField("_obj", BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Castclass, _fields[0] != null ? _fields[0].DeclaringType : _properties[0].DeclaringType);
        }

        for (int i = 0; i < _fields.Length; i++)
        {
            bool first = i == 0;
            bool last = i == (_fields.Length - 1);


            if (first && isStatic)
            {
                if (_fields[i] != null)
                    il.Emit(OpCodes.Ldsfld, _fields[i]);
                else
                    il.Emit(OpCodes.Call, _properties[i].GetGetMethod());
                continue;
            }
            if (!last)
            {
                if (_fields[i] != null)
                    il.Emit(OpCodes.Ldfld, _fields[i]);
                else
                    il.Emit(OpCodes.Callvirt, _properties[i].GetGetMethod());
                continue;
            }
            if (last)
            {
                il.Emit(OpCodes.Ldarg_1);

                if (_fields[i] != null)
                    il.Emit(OpCodes.Stfld, _fields[i]);
                else
                    il.Emit(OpCodes.Callvirt, _properties[i].GetSetMethod());

                il.Emit(OpCodes.Ret);
                break;
            }
        }

        return (Action<T>)dynMethod.CreateDelegate(typeof(Action<T>), this);

    }
}