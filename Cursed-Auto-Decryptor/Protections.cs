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
                    IList<Instruction> IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        if (IL[x].OpCode == OpCodes.Call &&
                            IL[x].Operand.ToString().Contains("<System.String>")) // TODO : Add More Check For More Accuracy
                        {
                            try
                            {
                                IMethod DecMethod = (IMethod)IL[x].Operand;
                                var ParamsC = DecMethod.GetParams().Count;
                                Context.Log.Information($"Detected Params : {ParamsC}");
                                var ReturnedParams = ParseParams(IL, x, ParamsC, DecMethod);
                                var Result = (string)((MethodInfo)Context.Ass.ManifestModule.ResolveMethod((int)DecMethod.MDToken.Raw)).Invoke(null, ReturnedParams);
                                Context.Log.Information($"Resorted String : {Result}");
                                IL[x] = new Instruction(OpCodes.Ldstr, Result);
                                for (int i = -ParamsC + x; i < x; i++)
                                {
                                    Context.Log.Information($"Removing Params : {IL[i].Operand} ..");
                                    IL.RemoveAt(i);
                                }
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
        public object[] ParseParams(IList<Instruction> IL, int x, int Count, IMethod DecMethod) // TODO : Make The Algorithm More Good ;)
        {
            int lol = -0;
            int lel = 0;
            object[] ParsedParams = new object[Count];
            for (int i = -Count + x; i < x; i++)
            {
                try
                {
                    if (IL[i].IsLdcI4())
                    {
                        // Converting Type MayBe params is uint or string or anything else :)
                        var MethParams = DecMethod.GetParams()[lel++].GetFullName();
                        ParsedParams[lol++] = Convert.ChangeType(IL[i].GetLdcI4Value(), Type.GetType(MethParams));
                    }
                    else
                    {
                        var MethParams = DecMethod.GetParams()[lel++].GetFullName();
                        ParsedParams[lol++] = Convert.ChangeType(IL[i].Operand, Type.GetType(MethParams));
                    }
                }
                catch
                {
                    lel--;
                    lol--;
                    if (IL[i].IsLdcI4())
                    {
                        var MethParams = DecMethod.GetParams()[lel].GetFullName();
                        ParsedParams[lol] = Convert.ChangeType((unchecked((uint)IL[i].GetLdcI4Value())), Type.GetType(MethParams));
                    }
                    else
                    {
                        var MethParams = DecMethod.GetParams()[lel].GetFullName();
                        ParsedParams[lol] = Convert.ChangeType((unchecked((uint)IL[i].Operand)), Type.GetType(MethParams));
                    }
                }
            }
            return ParsedParams;
        }
    }
}