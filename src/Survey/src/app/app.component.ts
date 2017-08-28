import { Component, ViewChild, OnInit, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { PostalService } from './postal.service';
import { PostalData } from './postal.Model';
import { LocalAuthority } from './authority.model';

@Component({
    providers: [PostalService],
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit {

    @ViewChild("autocomplete") public autocomplete: any;

    @Input() form: FormGroup;

    public isError: boolean;

    public hasVoted: boolean;

    public data: PostalData[] = [];

    public neighbors: LocalAuthority[] = [];

    public selectedLocalAuthority: string;

    public selectedArea: PostalData;

    public selectedPostCode: string;

    public isBusy: boolean;

    constructor(private postalService: PostalService) {
    }

    ngOnInit() {
    }

    ngAfterViewInit() {
        this.autocomplete.filterChange.asObservable()
            .do(() => {
                this.autocomplete.loading = true;
            })
            .filter(item => item.length > 2)
            .switchMap(value => this.postalService.findSimilarPost(value))
            .subscribe(item => {
                this.data = item;
                this.autocomplete.loading = false;
                this.autocomplete.toggle(true);
            });
    }

    public onSubmit(event: any): void {

        this.isError = this.neighbors.find(item => (item.ratingOne == null || item.ratingTwo == null) && item.active) != null;
        if (!this.isError) {
            this.postalService.save(this.selectedArea, this.neighbors);
            this.hasVoted = true;
        }
    }

    public onOpenPost(event: any): void {
        if (this.data == null ||
            this.data.length === 0) {
            event.preventDefault();
        }
    }

    public onValueChangePost(event: any): void {
        this.autocomplete.loading = false;
        this.selectedArea = this.data.find(item => item.postalCode.toUpperCase() === event.toUpperCase());
        if (this.selectedArea != null) {
            this.selectedLocalAuthority = this.selectedArea.borough;
            this.selectedPostCode = this.selectedArea.postalCode;
            this.postalService.findNeighbors(this.selectedArea.boroughCode)
                .do(() => {
                    this.isBusy = true;
                })
                .subscribe(item => {
                    for (let oneItem of item) {
                        oneItem.active = true;
                    }

                    this.neighbors = item;
                    this.isBusy = false;
                });
        }
    }

    public onFilterChangePost(event: any): void {
    }
}