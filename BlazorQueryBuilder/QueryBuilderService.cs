using System;

namespace BlazorQueryBuilder
{
    public class QueryBuilderService
    {
        private Type _parameterType;

        public void SetParameterType(Type parameterType) => _parameterType = parameterType;

        public Type GetParameterType() => _parameterType;
    }
}
