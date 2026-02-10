import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { authHttpInterceptorFn, provideAuth0 } from '@auth0/auth0-angular';
import { mergeApplicationConfig } from '@angular/core';
import { environment } from './environments/environment';
import { App } from './app/app';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

if (!environment.auth0.domain || !environment.auth0.clientId) {
  console.error("Auth0 configuration missing. Please check your environment.ts file.");
  console.error("Required environment variables:");
  console.error("- auth0.domain");
  console.error("- auth0.clientId");
  throw new Error("Auth0 domain and client ID must be set in environment.ts file");
}

if (!environment.auth0.domain.includes('.auth0.com') &&
    !environment.auth0.domain.includes('.us.auth0.com') &&
    !environment.auth0.domain.includes('.eu.auth0.com') &&
    !environment.auth0.domain.includes('.au.auth0.com')) {
  console.warn("Auth0 domain format might be incorrect. Expected format: your-domain.auth0.com");
}

const auth0Config = mergeApplicationConfig(appConfig, {
  providers: [
    provideAuth0({
      domain: environment.auth0.domain,
      clientId: environment.auth0.clientId,
      authorizationParams: {
        redirect_uri: window.location.origin,
        audience: environment.auth0.audience
      },
      httpInterceptor: {
        allowedList: [
          {
            uri: `${environment.apiUrl}/*`
          }
        ]
      }
    }),
    provideHttpClient(
      withInterceptors([authHttpInterceptorFn])
    )
  ]
});

bootstrapApplication(App, auth0Config).catch((err) =>
  console.error(err)
);
