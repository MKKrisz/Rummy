using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Rummy
{
    public static class PlayerInvokableContainer
    {
        public static List<PlayerInvokable> Methods = new List<PlayerInvokable>();
        

        public static void Init()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<MethodInfo> infos = new List<MethodInfo>();
            for (int i = 0; i < assemblies.Length; i++)
            {
                infos.AddRange(assemblies[i].GetTypes().SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes().OfType<PlayerInvokable>().Any()).ToList());
            }

            for (int i = 0; i < infos.Count(); i++)
            {
                PlayerInvokable p = infos[i].GetCustomAttribute<PlayerInvokable>();
                p.Info = infos[i];
                p.Params = p.Info.GetParameters();
                Methods.Add(p);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TurnEnder : Attribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class PlayerInvokable : Attribute
    {
        public string Name;
        public string Description;
        public MethodInfo Info;
        public ParameterInfo[] Params;

        public PlayerInvokable(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void Invoke(List<Object> parameters) {
            if (parameters.Count > Params.Length) { return;}
            
            for (int i = 0; i < Params.Length; i++) {
                if (!Params[i].IsOptional) {
                    if (i >= parameters.Count) { return;}
                    if (Params[i].ParameterType != parameters[i].GetType()) { return; }
                }

                if (Params[i].IsOptional) {
                    if (i >= parameters.Count) { parameters.Add(Params[i].DefaultValue);}

                    if (Params[i].ParameterType != parameters[i].GetType()) {
                        parameters[i] = Params[i].DefaultValue;
                    }
                }
            }
            
            Info.Invoke(null, parameters.ToArray());
        }
        

    }
}