using System.Reflection;
using System.Windows.Controls;
using ControleFinanceiroForms.Features.Investments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ControleFinanceiroForms.Tests;

[TestClass]
public class ViewInstantiationTests
{
    [TestMethod]
    public void AllUserControls_CanBeInstantiated_WithoutXamlExceptions()
    {
        var exceptions = new List<Exception>();
        
        var thread = new Thread(() =>
        {
            try
            {
                var assembly = typeof(InvestmentsView).Assembly;
                var viewTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(UserControl).IsAssignableFrom(t))
                    .ToList();

                foreach (var type in viewTypes)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(type);
                        Assert.IsNotNull(instance);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(new Exception($"Failed to instantiate {type.Name}", ex));
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exceptions.Any())
        {
            var messages = string.Join("\n", exceptions.Select(e => e.ToString()));
            Assert.Fail($"One or more views failed to instantiate:\n{messages}");
        }
    }
}
