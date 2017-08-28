import { Injectable } from '@angular/core';

import { Headers, Http, RequestOptions, ResponseContentType, RequestMethod } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { PostalData } from "./postal.model";
import { LocalAuthority } from "./authority.model";



@Injectable()
export class PostalService {
  constructor(private http: Http) {
  }

  public findSimilarPost(code): Observable<PostalData[]> {
    var data = { Code: code }
    return this.postJson('http://api.wikiled.com/Postal/Find', data)
        .map(item => item as PostalData[]);
  }

  public findNeighbors(code): Observable<LocalAuthority[]> {
    var data = { Code: code }
    return this.postJson('http://api.wikiled.com/Postal/Neighbors', data)
        .map(item => item as LocalAuthority[]);
  }

  public save(code: PostalData, result: LocalAuthority[]) {    
    var data = {
        PostCode: code.postalCode,
        Authority: code.boroughCode,
        Data: result };
    return this.postJson('/api/Survey', data).subscribe(item => item);
  }

  private postJson(url: string, data: any): Observable<any> {
    var headersMy = new Headers({ 'Content-Type': 'application/json' });
    var dataObj = JSON.stringify(data);
    var requestOptions = new RequestOptions({
      responseType: ResponseContentType.Json,
      method: RequestMethod.Post,
      body: dataObj,
      headers: headersMy
    });

    return this.http.post(url, dataObj, requestOptions)
      .map(response => {
        var js = response.json();
        return js.result;
      });
  }
}