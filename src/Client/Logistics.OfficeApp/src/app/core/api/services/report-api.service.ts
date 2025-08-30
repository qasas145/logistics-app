import {HttpResponse, HttpHeaders} from "@angular/common/http";
import {Observable} from "rxjs";
import {map} from "rxjs/operators";
import {ApiBase} from "../api-base";
import {
  Result,
  PagedResult,
  SearchableQuery,
  GetLoadsReportQuery,
  LoadsReportDto,
  DriversReportDto,
  FinancialsReportDto,
} from "../models";

export interface GetLoadsReportExportQuery extends GetLoadsReportQuery {
  format: "csv" | "xlsx";
}

export class ReportApiService extends ApiBase {
  getLoadsReport(query?: GetLoadsReportQuery): Observable<PagedResult<LoadsReportDto>> {
    return this.get(`/reports/loads?${this.stringfyQuery(query)}`);
  }

  exportLoadsReport(format: string): Observable<HttpResponse<Blob>> {
    const url = `/reports/loads/export?format=${format}`;
    const xhr = new XMLHttpRequest();
    xhr.open("GET", this.apiUrl + url, true);
    xhr.responseType = "blob";
    
    return new Observable<HttpResponse<Blob>>(observer => {
      xhr.onload = () => {
        if (xhr.status === 200) {
          const response = new HttpResponse({
            body: xhr.response,
            headers: new HttpHeaders(xhr.getAllResponseHeaders()),
            status: xhr.status,
            statusText: xhr.statusText,
            url: xhr.responseURL
          });
          observer.next(response);
          observer.complete();
        } else {
          observer.error(new Error(`Export failed with status ${xhr.status}`));
        }
      };
      xhr.onerror = () => observer.error(new Error("Export request failed"));
      xhr.send();
    });
  }

  getDriversReport(query?: SearchableQuery): Observable<any> {
    return this.get(`/reports/drivers?${this.stringfySearchableQuery(query)}`);
  }

  getFinancialsReport(query?: SearchableQuery): Observable<Result<FinancialsReportDto>> {
    return this.get(`/reports/financials?${this.stringfySearchableQuery(query)}`);
  }
}
