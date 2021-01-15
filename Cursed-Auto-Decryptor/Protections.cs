using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cursed_Auto_Decryptor
{
    public class Protections
    {
        public List<Instruction> ToRemoveInst = new List<Instruction>();
        public Protections(Context Context)
        {
            DecryptConstants(Context);
            Context.Save();
        }
        public void DecryptConstants(Context Context)
        {
            foreach (var TypeDef in Context.Module.Types.Where(x => x.HasMethods && !x.IsGlobalModuleType))
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                {
                    IList<Instruction> IL = MethodDef.Body.Instructions.ToList();
                    for (int x = 0; x < IL.Count; x++)
                    {
                        if (IL[x].OpCode == OpCodes.Call &&
                            IL[x].Operand.ToString().Contains("<System.String>")) // TODO : Add More Check For More Accuracy
                        {
                            try
                            {
                                int IndexIs = x;
                                IMethod DecMethod = (IMethod)IL[x].Operand;
                                var ParamsC = DecMethod.GetParams().Count;
                                if (HaveAntiInvoke(DecMethod)) { Context.Log.Information("The Decryption Method Have AntiInvoke Please Remove Then Continue");Context.Log.Information("Don't Know How Check This : https://github.com/obfuscators-2019/AntiInvokeDetection"); Console.ReadKey(); Environment.Exit(0); }
                                Context.Log.Information($"Detected Params : {ParamsC}");
                                var ReturnedParams = ParseParams(IL, x, ParamsC, DecMethod);
                                var Result = (string)((MethodInfo)Context.Ass.ManifestModule.ResolveMethod((int)DecMethod.MDToken.Raw)).Invoke(null, ReturnedParams);
                                Context.Log.Information($"Resorted String : {Result}");
                                IL[x] = OpCodes.Ldstr.ToInstruction(Result);
                                foreach (var i in ToRemoveInst)
                                    IL.Remove(i);
                                MethodDef.Body = new CilBody(MethodDef.Body.InitLocals, IL, MethodDef.Body.ExceptionHandlers, MethodDef.Body.Variables);
                            }
                            catch (Exception e)
                            {
                                Context.Log.Error(e.Message);
                            }
                        }
                    }
                }
            }
        }
        public object[] ParseParams(IList<Instruction> IL, int x, int Count, IMethod DecMethod)
        {
            int lol = -0; // lmk if their a better solution 🙂
            int lel = 0;
            object[] ParsedParams = new object[Count];
            for (int i = -Count + x; i < x; i++) {
                if (lol == -0) {
                    if (IL[i].IsLdcI4()) {
                        ParsedParams[lol++] = Convert.ChangeType(unchecked((uint)IL[i].GetLdcI4Value()), Type.GetType(DecMethod.GetParams()[lel++].GetFullName()));
                    }
                    else {
                        ParsedParams[lol++] = Convert.ChangeType(IL[i].Operand, Type.GetType(DecMethod.GetParams()[lel++].GetFullName()));
                    }
                }
                else {
                    try { ParsedParams[lol++] = Activator.CreateInstance(Type.GetType(DecMethod.GetParams()[lel++].GetFullName())); }
                    catch { lel--; lol--; ParsedParams[lol++] = Convert.ChangeType(0, Type.GetType(DecMethod.GetParams()[lel++].GetFullName())); }
                }
                ToRemoveInst.Add(IL[i]);
            }
            return ParsedParams;
        }
        public bool HaveAntiInvoke(IMethod DecMethod)
        {
            foreach (var x in DecMethod.ResolveMethodDef().Body.Instructions)
                if (x.OpCode == OpCodes.Call && x.Operand.ToString().Contains("GetCallingAssembly")) { return true; }
            return false;
        }
    }
}