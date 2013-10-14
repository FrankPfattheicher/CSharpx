using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.CSharp;

namespace CSharpx.TypeProviders
{
  public class TypeProvider
  {
    protected List<TypeProperty> Properties;

    private Type type;
    public Type Type
    {
      get { return type ?? (type = CreateType()); }
    }

    public dynamic CreateInstance()
    {
      return Convert.ChangeType(Activator.CreateInstance(Type, new object[]{ this }), Type);
    }

    // ReSharper disable once UnusedMember.Local
    private Type CreateType()
    {
      var classType = new CodeTypeDeclaration("ProvidedType_" + Guid.NewGuid().ToString("N")) { IsClass = true };
      classType.BaseTypes.Add(GetType());

      var instanceField = new CodeMemberField
      {
        Name = "Instance",
        Attributes = MemberAttributes.Private | MemberAttributes.Final,
        Type = new CodeTypeReference(GetType())
      };
      classType.Members.Add(instanceField);

      var constructor = new CodeConstructor { Attributes = MemberAttributes.Public | MemberAttributes.Final };
      constructor.Parameters.Add(new CodeParameterDeclarationExpression(GetType(), "instance"));

      var instanceReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "Instance");
      constructor.Statements.Add(new CodeAssignStatement(instanceReference, new CodeArgumentReferenceExpression("instance")));

      classType.Members.Add(constructor);

      foreach (var property in Properties)
      {
        if (property.DataType == typeof(object))  // TODO: Provider
          continue;

        var prop = new CodeMemberProperty
        {
          Name = property.Name,
          Type = new CodeTypeReference(property.DataType),
          Attributes = MemberAttributes.Public,
          HasGet = true,
          //HasSet = true
        };

        if (property.DataType == typeof(bool))
        {
          prop.GetStatements.Add(new CodeSnippetStatement("return (bool)Instance.GetPropertyValue(\"" + property.Name + "\")"));
        }
        else if (property.DataType == typeof (string))
        {
          prop.GetStatements.Add(new CodeSnippetStatement("return Instance.GetPropertyValue(\"" + property.Name + "\").ToString();"));
        }
        else
        {
          var statement = "return Convert.ChangeType(Instance.GetPropertyValue(\"" + property.Name + "\"), typeof(" + property.DataType.FullName + "));";
          prop.GetStatements.Add(new CodeSnippetStatement(statement));
        }
        classType.Members.Add(prop);
      }

      var cnamespace = new CodeNamespace("CSharpx.TypeProviders");
      cnamespace.Imports.Add(new CodeNamespaceImport("System"));
      cnamespace.Types.Add(classType);

      var compileUnit = new CodeCompileUnit();
      compileUnit.Namespaces.Add(cnamespace);
      compileUnit.ReferencedAssemblies.Add("System.dll");
      compileUnit.ReferencedAssemblies.Add(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "CSharpx.TypeProviders.dll"));
      compileUnit.ReferencedAssemblies.Add(GetType().Assembly.Location);

      var compilerParameters = new CompilerParameters { GenerateInMemory = true };
      var cdp = new CSharpCodeProvider();
      var results = cdp.CompileAssemblyFromDom(compilerParameters, new[] { compileUnit });
      if (results.Errors.HasErrors)
      {
        var errorMessages = new StringBuilder();
        foreach (var err in results.Errors)
          errorMessages.AppendLine(err.ToString());
        Debug.Print(errorMessages.ToString());
        return null;
      }

      var assembly = results.CompiledAssembly;
      var exportedTypes = assembly.GetExportedTypes();
      var providedType = exportedTypes[0];
      return providedType;
    }

  }
}