using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace QIRC.CSharp.SDIL
{
    public class MethodBodyReader
    {
        public List<ILInstruction> instructions = null;
        protected Byte[] il = null;
        private MethodInfo mi = null;

        #region il read methods
        private Int32 ReadInt16(Byte[] _il, ref Int32 position)
        {
            return ((il[position++] | (il[position++] << 8)));
        }
        private UInt16 ReadUInt16(Byte[] _il, ref Int32 position)
        {
            return (UInt16)((il[position++] | (il[position++] << 8)));
        }
        private Int32 ReadInt32(Byte[] _il, ref Int32 position)
        {
            return (((il[position++] | (il[position++] << 8)) | (il[position++] << 0x10)) | (il[position++] << 0x18));
        }
        private UInt64 ReadInt64(Byte[] _il, ref Int32 position)
        {
            return (UInt64)(((il[position++] | (il[position++] << 8)) | (il[position++] << 0x10)) | (il[position++] << 0x18) | (il[position++] << 0x20) | (il[position++] << 0x28) | (il[position++] << 0x30) | (il[position++] << 0x38));
        }
        private Double ReadDouble(Byte[] _il, ref Int32 position)
        {
            return (((il[position++] | (il[position++] << 8)) | (il[position++] << 0x10)) | (il[position++] << 0x18) | (il[position++] << 0x20) | (il[position++] << 0x28) | (il[position++] << 0x30) | (il[position++] << 0x38));
        }
        private SByte ReadSByte(Byte[] _il, ref Int32 position)
        {
            return (SByte)il[position++];
        }
        private Byte ReadByte(Byte[] _il, ref Int32 position)
        {
            return (Byte)il[position++];
        }
        private Single ReadSingle(Byte[] _il, ref Int32 position)
        {
            return (Single)(((il[position++] | (il[position++] << 8)) | (il[position++] << 0x10)) | (il[position++] << 0x18));
        }
        #endregion

        /// <summary>
        /// Constructs the array of ILInstructions according to the IL byte code.
        /// </summary>
        /// <param name="module"></param>
        private void ConstructInstructions(Module module)
        {
            Byte[] il = this.il;
            Int32 position = 0;
            instructions = new List<ILInstruction>();
            Globals.LoadOpCodes();
            while (position < il.Length)
            {
                ILInstruction instruction = new ILInstruction();

                // get the operation code of the current instruction
                OpCode code = OpCodes.Nop;
                UInt16 value = il[position++];
                if (value != 0xfe)
                {
                    code = Globals.singleByteOpCodes[(Int32)value];
                }
                else
                {
                    value = il[position++];
                    code = Globals.multiByteOpCodes[(Int32)value];
                    value = (UInt16)(value | 0xfe00);
                }
                instruction.Code = code;
                instruction.Offset = position - 1;
                Int32 metadataToken = 0;
                // get the operand of the current operation
                switch (code.OperandType)
                {
                    case OperandType.InlineBrTarget:
                        metadataToken = ReadInt32(il, ref position);
                        metadataToken += position;
                        instruction.Operand = metadataToken;
                        break;
                    case OperandType.InlineField:
                        metadataToken = ReadInt32(il, ref position);
                        instruction.Operand = module.ResolveField(metadataToken);
                        break;
                    case OperandType.InlineMethod:
                        metadataToken = ReadInt32(il, ref position);
                        try
                        {
                            instruction.Operand = module.ResolveMethod(metadataToken);
                        }
                        catch
                        {
                            try
                            {
                                instruction.Operand = module.ResolveMember(metadataToken);
                            }
                            catch (Exception)
                            {
                                //Try generic method
                                try
                                {
                                    instruction.Operand = module.ResolveMethod(metadataToken, mi.DeclaringType.GetGenericArguments(), mi.GetGenericArguments());
                                }
                                catch (Exception)
                                {
                                    //Try generic member
                                    try
                                    {
                                        instruction.Operand = module.ResolveMember(metadataToken, mi.DeclaringType.GetGenericArguments(), mi.GetGenericArguments());
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                }

                            }
                        }
                        break;
                    case OperandType.InlineSig:
                        metadataToken = ReadInt32(il, ref position);
                        instruction.Operand = module.ResolveSignature(metadataToken);
                        break;
                    case OperandType.InlineTok:
                        metadataToken = ReadInt32(il, ref position);
                        try
                        {
                            instruction.Operand = module.ResolveType(metadataToken);
                        }
                        catch
                        {

                        }
                        // SSS : see what to do here
                        break;
                    case OperandType.InlineType:
                        metadataToken = ReadInt32(il, ref position);
                        // now we call the ResolveType always using the generic attributes type in order
                        // to support decompilation of generic methods and classes

                        // thanks to the guys from code project who commented on this missing feature
                        try
                        {
                            instruction.Operand = module.ResolveType(metadataToken);
                        }
                        catch (Exception)
                        {
                            instruction.Operand = module.ResolveType(metadataToken, this.mi.DeclaringType.GetGenericArguments(), this.mi.GetGenericArguments());
                        }
                        break;
                    case OperandType.InlineI:
                        {
                            instruction.Operand = ReadInt32(il, ref position);
                            break;
                        }
                    case OperandType.InlineI8:
                        {
                            instruction.Operand = ReadInt64(il, ref position);
                            break;
                        }
                    case OperandType.InlineNone:
                        {
                            instruction.Operand = null;
                            break;
                        }
                    case OperandType.InlineR:
                        {
                            instruction.Operand = ReadDouble(il, ref position);
                            break;
                        }
                    case OperandType.InlineString:
                        {
                            metadataToken = ReadInt32(il, ref position);
                            instruction.Operand = module.ResolveString(metadataToken);
                            break;
                        }
                    case OperandType.InlineSwitch:
                        {
                            Int32 count = ReadInt32(il, ref position);
                            Int32[] casesAddresses = new Int32[count];
                            for (Int32 i = 0; i < count; i++)
                            {
                                casesAddresses[i] = ReadInt32(il, ref position);
                            }
                            Int32[] cases = new Int32[count];
                            for (Int32 i = 0; i < count; i++)
                            {
                                cases[i] = position + casesAddresses[i];
                            }
                            break;
                        }
                    case OperandType.InlineVar:
                        {
                            instruction.Operand = ReadUInt16(il, ref position);
                            break;
                        }
                    case OperandType.ShortInlineBrTarget:
                        {
                            instruction.Operand = ReadSByte(il, ref position) + position;
                            break;
                        }
                    case OperandType.ShortInlineI:
                        {
                            instruction.Operand = ReadSByte(il, ref position);
                            break;
                        }
                    case OperandType.ShortInlineR:
                        {
                            instruction.Operand = ReadSingle(il, ref position);
                            break;
                        }
                    case OperandType.ShortInlineVar:
                        {
                            instruction.Operand = ReadByte(il, ref position);
                            break;
                        }
                    default:
                        {
                            throw new Exception("Unknown operand type.");
                        }
                }
                instructions.Add(instruction);
            }
        }

        public Object GetRefferencedOperand(Module module, Int32 metadataToken)
        {
            AssemblyName[] assemblyNames = module.Assembly.GetReferencedAssemblies();
            for (Int32 i=0; i<assemblyNames.Length; i++)
            {
                Module[] modules = Assembly.Load(assemblyNames[i]).GetModules();
                for (Int32 j=0; j<modules.Length; j++)
                {
                    try
                    {
                        Type t = modules[j].ResolveType(metadataToken);
                        return t;
                    }
                    catch
                    {

                    }

                }
            }
            return null;
        //System.Reflection.Assembly.Load(module.Assembly.GetReferencedAssemblies()[3]).GetModules()[0].ResolveType(metadataToken)

        }
        /// <summary>
        /// Gets the IL code of the method
        /// </summary>
        /// <returns></returns>
        public String GetBodyCode()
        {
            String result = "";
            if (instructions != null)
            {
                for (Int32 i = 0; i < instructions.Count; i++)
                {
                    result += instructions[i].GetCode() + "\n";
                }
            }
            return result;

        }

        /// <summary>
        /// MethodBodyReader constructor
        /// </summary>
        /// <param name="mi">
        /// The System.Reflection defined MethodInfo
        /// </param>
        public MethodBodyReader(MethodInfo mi)
        {
            this.mi = mi;
            if (mi.GetMethodBody() != null)
            {
                il = mi.GetMethodBody().GetILAsByteArray();
                ConstructInstructions(mi.Module);
            }
        }
    }
}
