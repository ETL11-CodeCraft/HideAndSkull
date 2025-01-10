using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HideAndSkull.Lobby.Utilities
{
    /// <summary>
    /// 자동 의존성 주입을 위한 특성
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ResolveAttribute : Attribute
    {

    }

    public static class ResolvePrefixTable
    {
        public static string GetPrefix(Type type)
        {
            if(s_prefixes.TryGetValue(type, out var prefix)) 
                return prefix;

            return string.Empty;
        }

        private static Dictionary<Type, string> s_prefixes = new Dictionary<Type, string>()
        {
            {typeof(Transform), "" },
            {typeof(RectTransform), "" },
            {typeof(GameObject), "" },
            {typeof(TMP_Text), "Text (TMP) - " },
            {typeof(TextMeshProUGUI), "Text (TMP) - " },
            {typeof(TextMeshPro), "Text (TMP) - " },
            {typeof(TMP_InputField), "InputField (TMP) - " },
            {typeof(Image), "Image - " },
            {typeof(Button), "Button - " }
        };
    }

    /// <summary>
    /// 모든 자식들을 탐색하여 의존성 주입이 가능한 필드의 의존성을 알아서 해결해주는 기반클래스
    /// </summary>
    public abstract class ComponentResolvingBehavior : MonoBehaviour
    {
        protected virtual void Awake()
        {
            ResolveAll();
        }

        private void ResolveAll()
        {
            Type type = GetType();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            StringBuilder stringBuilder = new StringBuilder(40);

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                ResolveAttribute resolveAttribute = fieldInfos[i].GetCustomAttribute<ResolveAttribute>();

                if (resolveAttribute != null)
                {
                    stringBuilder.Clear();
                    string prefix = ResolvePrefixTable.GetPrefix(fieldInfos[i].FieldType);
                    stringBuilder.Append(prefix);
                    string fieldName = fieldInfos[i].Name;
                    bool isFirstCharacter = true;

                    //_camelCase -> PascalCase
                    for (int j = 0; j < fieldName.Length; j++)
                    {
                        if (isFirstCharacter)
                        {
                            if (fieldName[j].Equals('_'))
                                continue;

                            stringBuilder.Append(char.ToUpper(fieldName[j]));
                            isFirstCharacter = false;
                        }
                        else
                        {
                            stringBuilder.Append(fieldName[j]);
                        }
                    }

                    Transform child = transform.FindChildReculsively(stringBuilder.ToString());

                    if(child)
                    {
                        Component childComponenet = child.GetComponent(fieldInfos[i].FieldType);
                        fieldInfos[i].SetValue(this, childComponenet);
                    }
                    else
                    {
                        Debug.LogError($"[{name}] : Can not resolve field {fieldInfos[i].Name}");
                    }
                }
            }
        }
    }
}