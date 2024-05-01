namespace RegionDeployer
{
    using System;
    using System.Linq;
    using System.Text;

    public class ErrorUtilities
    {
        public servicelogdto createlog(Exception exception, string errorscontext, bool sendadminalert)
        {
            string message = getallinnermessages(exception);
            string stacktrace = getallinnerstacktraces(exception);

            return new servicelogdto
            {
                Timeof = DateTime.UtcNow,
                LogsLevel = errorscontext,
                Message = message,
                SendAdminAlert = sendadminalert,
                Stacktrace = stacktrace
            };
        }

        private string getallinnermessages(Exception exception)
        {
            var stringbuilder = new StringBuilder();

            do
            {                
                stringbuilder.AppendLine(exception.Message);
                stringbuilder.AppendLine("-----------------");

                if (exception is DbEntityValidationException validationexception)
                {
                    string validations = validationexception.EntityValidationErrors
                        .Select(currentvalidation =>
                        {
                            var validationbuilder = new StringBuilder();

                            foreach (var item in currentvalidation.ValidationErrors)
                            {
                                validationbuilder.AppendLine(item.ErrorMessage);
                            }
                            return validationbuilder.ToString();
                        })
                        .Aggregate<string>((currentitem, accumulate) => accumulate += currentitem);

                    stringbuilder.Append(validations);
                    stringbuilder.AppendLine("-----------------");
                }
                exception = exception.InnerException;
            }

            while (exception != null);
            return stringbuilder.ToString();
        }

        public string getallinnerstacktraces(Exception exception)
        {
            var stringbuilder = new StringBuilder();

            do
            {
                stringbuilder.AppendLine(exception.StackTrace);
                stringbuilder.AppendLine("-----------------");
                exception = exception.InnerException;
            }

            while (exception != null);
            return stringbuilder.ToString();
        }
    }
}

