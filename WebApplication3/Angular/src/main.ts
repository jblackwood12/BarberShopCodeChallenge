import { enableProdMode, Component } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

//// added this.
//@Component({
//  selector: 'app-root',
//  templateUrl: './index.html',
//  styleUrls: ['./styles.css']
//})

// added this
//export class AppComponentMain { }

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error(err));
