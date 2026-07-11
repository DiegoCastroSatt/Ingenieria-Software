import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { HealthResponse } from '../models/health.models';

@Injectable({ providedIn: 'root' })
export class HealthService {
  private readonly http = inject(HttpClient);
  health(): Observable<HealthResponse> { return this.http.get<HealthResponse>(`${enviroment.apiUrl}/health`); }
}
