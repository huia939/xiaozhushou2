using System;
using System.Windows.Forms;

namespace Common
{
    public class ActionInvoke
    {
        private Control m_control;

        public ActionInvoke(Control control)
        {
            m_control = control;
        }

        public T Invoke<T>(Func<T> func)
        {
            T source = default(T);
            try
            {
                m_control.Invoke(new Action(() =>
                {
                    try
                    {
                        source = func.Invoke();
                    }
                    catch (Exception ex) { }
                }));
            }
            catch (Exception ex) { }
            return source;
        }

        public Result Invoke(Action func)
        {
            Result result = Result.Default;
            try
            {
                m_control.Invoke(new Action(() =>
                {
                    try
                    {
                        func.Invoke();
                    }
                    catch (Exception ex)
                    {
                        result = Result.Error(ex.Message);
                    }
                }));
            }
            catch (Exception ex)
            {
                result = Result.Error(ex.Message);
            }
            return result;
        }
    }

    public static class ActionInvokeHelper
    {
        public static T Invoke<T>(this Control control, Func<T> func)
        {
            return new ActionInvoke(control).Invoke(func);
        }

        public static Result Invoke(this Control control, Action func)
        {
            return new ActionInvoke(control).Invoke(func);
        }
    }
}
