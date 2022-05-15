using System;
using System.Threading.Tasks;
using walkwards_api.Utilities;

namespace walkwards_api_tests
{
    public static class Helpers
    {
        //get task and name of error and check if task return concert error
        public static async Task<bool> CheckTaskError(Task task, string errorName)
        {
            try
            {
                await task;
            }
            catch (CustomError ce)
            {
                return ce.Name == errorName;
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(AggregateException)) return false;
                
                if(ex.InnerException is CustomError ce)
                {
                    return ce.Name == errorName;
                    
                }

                return false;
            }

            return false;
        }
    }
}