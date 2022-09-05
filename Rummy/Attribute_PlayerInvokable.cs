using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Rummy
{
    public static class PlayerInvokableContainer
    {
        public static List<PlayerInvokable> Methods = new List<PlayerInvokable>();
        

        /* SUGGESTION: Call Init() in static constructor
        static PlayerInvokableContainer()
        {
            Init();
        }*/

        public static void Init()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<MethodInfo> infos = new List<MethodInfo>();
            for (int i = 0; i < assemblies.Length; i++)
            {
                //gets all methods with "PlayerInvokable" attribute in current assembly
                infos.AddRange(assemblies[i].GetTypes().SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes().OfType<PlayerInvokable>().Any()).ToList());
            }

            for (int i = 0; i < infos.Count(); i++)
            {
                //Gets the attribute
                PlayerInvokable p = infos[i].GetCustomAttribute<PlayerInvokable>();
                if(infos[i].IsStatic == false)
                    throw new Exception($"Method {infos[i]} is marked with {nameof(PlayerInvokable)}, but is not static.");
                //sets attribute's MethodInfo field
                p.Info = infos[i];
                //sets attribute's parameters field
                p.Params = p.Info.GetParameters();
                //adds attribute to the bunch
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
        
        public PlayerInvokable(){}
        public void Invoke(List<Object> parameters) {
            //if more parameters are given, return
            if (parameters.Count > Params.Length) { return;}
            
            for (int i = 0; i < Params.Length; i++) {
                if (!Params[i].IsOptional) {
                    //if required parameter does not have a value given, return
                    if (i >= parameters.Count) { return;}
                    
                    //if there is a type mismatch between required parameter and given value, return
                    if (Params[i].ParameterType != parameters[i].GetType()) { return; }
                }

                if (Params[i].IsOptional) {
                    //if optional parameter does not have a value given, use default value
                    if (i >= parameters.Count) { parameters.Add(Params[i].DefaultValue);}
                    
                    //if there is a type mismatch, use default parameter
                    if (parameters[i] == null || Params[i].ParameterType != parameters[i].GetType()) {
                        parameters[i] = Params[i].DefaultValue;
                    }
                }
            }
            //invoke the method using the given parameters, if above requirements match
            Info.Invoke(null, parameters.ToArray());
        }
        

    }
}
