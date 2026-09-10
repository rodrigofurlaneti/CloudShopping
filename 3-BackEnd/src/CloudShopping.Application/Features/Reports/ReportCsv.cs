namespace CloudShopping.Application.Features.Reports;
public static class ReportCsv
{
 public static string Cell(string value)
 {
  var trim=value.TrimStart();if(trim.Length>0&&"=+-@".Contains(trim[0]))value="'"+value;
  return "\""+value.Replace("\"","\"\"")+"\"";
 }
}
