using System;

using UnityEngine;
using UnityEngine.Serialization;


namespace OSG
{
    [Serializable]
    public struct MaterialParameter
    {
        public enum EPameterType
        {
            Float,
            Color,
            Int,
            Keyword
        }
        public string name;
        [FormerlySerializedAs("pameterType")]
        public EPameterType parameterType;
        public float floatValue;
        public Color colorValue;
        public int intValue;
        public bool keywordValue;

        private int? nameID;

        public override string ToString()
        {
            string value;
            switch (parameterType)
            {
                case EPameterType.Float:
                    value = floatValue.ToString("0.00");
                    break;
                case EPameterType.Color:
                    value = colorValue.ToString();
                    break;
                case EPameterType.Int:
                    value = intValue.ToString();
                    break;
                case EPameterType.Keyword:
                    value = keywordValue.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return parameterType + " " + name + " = " + value;
        }
        
        public MaterialParameter Zero()
        {
            MaterialParameter z = new MaterialParameter();
            z.name = name;
            z.nameID = nameID;
            z.parameterType = parameterType;
            
            switch (parameterType)
            {
                case EPameterType.Float:
                    floatValue = 0;
                    break;
                case EPameterType.Color:
                    colorValue = new Color(0,0,0,0);
                    break;
                case EPameterType.Int:
                    intValue=0;
                    break;
                case EPameterType.Keyword:
                    keywordValue=false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return z;
        }
        
        

        public MaterialParameter(MaterialParameter other) : this()
        {
            name = other.name;
            parameterType = other.parameterType;
            nameID = other.NameID;
            switch (parameterType)
            {
                case EPameterType.Float:
                    floatValue = other.floatValue;
                    break;
                case EPameterType.Color:
                    colorValue = other.colorValue;
                    break;
                case EPameterType.Int:
                    intValue = other.intValue;
                    break;
                case EPameterType.Keyword:
                    keywordValue = other.keywordValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public MaterialParameter(MaterialParameter other, MaterialPropertyBlock material) : this()
        {
            name = other.name;
            parameterType = other.parameterType;
            nameID = other.NameID;
            switch (parameterType)
            {
                case EPameterType.Float:
                    floatValue = material.GetFloat(nameID.Value);
                    break;
                case EPameterType.Color:
                    colorValue = material.GetVector(nameID.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public MaterialParameter(MaterialParameter other, Material material) : this()
        {
            name = other.name;
            parameterType = other.parameterType;
            nameID = other.NameID;
            switch (parameterType)
            {
                case EPameterType.Float:
                    floatValue = material.GetFloat(nameID.Value);
                    break;
                case EPameterType.Color:
                    colorValue = material.GetColor(nameID.Value);
                    break;
                case EPameterType.Int:
                    intValue = material.GetInt(nameID.Value);
                    break;
                case EPameterType.Keyword:
                    keywordValue = material.IsKeywordEnabled(name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

        public int NameID
        {
            get
            {
#if !UNITY_EDITOR
                if(!nameID.HasValue)
#endif
                {
                    nameID = Shader.PropertyToID(name);
                }
                return nameID.Value;
            }
        }
        
        public void SetMaterial(MaterialPropertyBlock material)
        {
            switch (parameterType)
            {
                case EPameterType.Float:
                    material.SetFloat(NameID, floatValue);
                    break;
                case EPameterType.Color:
                    material.SetColor(NameID, colorValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
       
        public  void SetMaterial(Material material)
        {
            switch (parameterType)
            {
                case EPameterType.Float:
                    material.SetFloat(NameID, floatValue);
                    break;
                case EPameterType.Color:
                    material.SetColor(NameID, colorValue);
                    break;
                case EPameterType.Int:
                    material.SetInt(NameID, intValue);
                    break;
                case EPameterType.Keyword:
                    if(keywordValue)
                    {
                        material.EnableKeyword(name);
                    }
                    else
                    {
                        material.DisableKeyword(name);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public string GetMaterialStatus(Material material)
        {
            switch(parameterType)
            {
                case EPameterType.Float: return name + " = " + material.GetFloat(name).ToString("0.00");
                case EPameterType.Color: return name + " = " + material.GetColor(name).ToHex();
                case EPameterType.Int: return name + " = " + material.GetInt(name);
                case EPameterType.Keyword: return  name + " = " + material.IsKeywordEnabled(name);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetMaterialPropertyBlockStatus(MaterialPropertyBlock material)
        {
            switch (parameterType)
            {
                case EPameterType.Float: return name + " = " + material.GetFloat(name).ToString("0.00");
                case EPameterType.Color: return name + " = " + material.GetVector(name);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }        

        public void Add(MaterialParameter p1, float f)
        {
#if UNITY_EDITOR
            if (NameID != p1.NameID)
            {
                throw new ArgumentException("Cant make a new parameter from 2 parameters of different name (" + name + " and " + p1.name +")");
            }
            if(parameterType != p1.parameterType)
            {
                throw new ArgumentException("Cant make a new parameter from 2 parameters of different type (" + name + " with " + parameterType + " and " + p1.parameterType+")");
            }
#endif
            switch (parameterType)
            {
                case EPameterType.Float:
                    floatValue += p1.floatValue * f;
                    break;
                case EPameterType.Color:
                    colorValue += p1.colorValue * f;
                    break;
                case EPameterType.Int:
                    intValue += (int)(p1.intValue * f);
                    break;
                case EPameterType.Keyword:
                    if(f > 0.5f)
                        keywordValue = p1.keywordValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }            
        }
    }
}
