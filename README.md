# DynamicQuery
ASP.NET MVC 动态查询框架

#更新
1. 扩展QueryOperator.IN操作
2. 修复Get提交表单无法使用exinclude的BUG

#使用

注册模型绑定
ModelBinders.Binders.Add(typeof(QueryDescriptor), new QueryDescriptorBinder());

页面上使用HTML扩展
@Html.QueryDropdownList("PhysicalStatus", new List<SelectListItem>() {
   new SelectListItem() { Text = "0和1", Value = "0,1" },
   new SelectListItem() { Text = "2和3", Value = "2,3" },
   new SelectListItem() { Text = "3以后", Value = "3,4,5,6" },
   new SelectListItem() { Text = "空条件测试", Value = "" }
},DynamicQuery.QueryOperator.IN,"int",null,new { })

使用IQueryable<T>扩展
.Query(where, out Total, out totalitems)

