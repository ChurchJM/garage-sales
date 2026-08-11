function modalLogic() {
    return {
        bsModal: null,
        isEditing: false,
        saving: false,

        currentUser: '',

        form: {
            id: 0,
            owner: '',
            saleType: '',
            street: '',
            zip: '',
            description: '',
            schedules: [],
            featuredItems: []
        },

        lookups: {
                    garageSaleTypes: [],
                    itemCategories: []
                },

        async initModal() {
            this.bsModal = new bootstrap.Modal(document.getElementById('editSaleModal'));
            await this.fetchLookups();
            await this.fetchCurrentUser();
        },

        async fetchLookups() {
            try {
                const response = await fetch('https://localhost:7285/api/lookups');
                if (response.ok) {
                    const data = await response.json();
                    // Map JSON keys safely to Alpine state
                    this.lookups.garageSaleTypes = data.garageSaleTypes || [];
                    this.lookups.itemCategories = data.itemCategories || [];
                }
            } catch (err) {
                console.error('Failed to fetch lookups:', err);
            }
        },

        async fetchCurrentUser(){
            try {
                console.warn('fetching current user');
                const response = await fetch('https://localhost:7285/api/auth/me', {
                            credentials: 'include'
                        });
                if (response.ok) {
                    const data = await response.json();
                    console.log(data);
                    this.currentUser = data.userName;
                }
            } catch (err) {
                console.error('Failed to fetch current user info.', err);
            }
        },

        // UUIDs serve as temporary client-side keys for Alpine to track.
        addSchedule() {
            this.form.schedules.push({ _key: crypto.randomUUID(), from: '', to: '' });
        },
        removeSchedule(index) {
            this.form.schedules.splice(index, 1);
        },

        addFeaturedItem() {
            this.form.featuredItems.push({ _key: crypto.randomUUID(), category: '', name: '', description: '', price: 0.0 });
        },
        removeFeaturedItem(index) {
            this.form.featuredItems.splice(index, 1);
        },

        openCreateModal() {
            console.warn('opening create modal.');
            this.isEditing = false;
            this.form = {
                id: 0,
                owner: this.currentUser,
                saleType: '',
                street: '',
                zip: '',
                description: '',
                schedules: [{ from: '', to: '' }],
                featuredItems: []
            };
            this.bsModal.show();
        },

        openEditModal(sale) {
            console.log(sale);
            this.isEditing = true;
            this.form = {
                id: sale.id,
                owner: sale.owner,
                saleType: sale.saleType || '',
                street: sale.street || '',
                zip: sale.zip || '',
                description: sale.description || '',
                schedules: sale.schedules ? sale.schedules.map(s => ({
                    _key: crypto.randomUUID(),
                    from: toLocalDatetimeString(s.from),
                    to: toLocalDatetimeString(s.to)
                })) : [],
                featuredItems: sale.featuredItems ? sale.featuredItems.map(i => ({
                    _key: crypto.randomUUID(),
                    category: i.category || '',
                    name: i.name || '',
                    description: i.description || '',
                    price: i.price || ''
                })) : []
            };
            this.bsModal.show();
        },

        async saveSale() {
            console.warn('--- SAVE SALE TRIGGERED ---');
            
            if (this.form.schedules.length === 0) {
                alert('Please add at least one schedule date.');
                return;
            }

            this.saving = true;
            const url = this.isEditing
                ? `https://localhost:7285/api/garagesales/${this.form.id}`
                : 'https://localhost:7285/api/garagesales';

            const method = this.isEditing ? 'PUT' : 'POST';

            // Convert dates back to full ISO strings for C# endpoint.
            const payload = {
                ...this.form,
                schedules: this.form.schedules.map(s => ({
                    from: s.from ? new Date(s.from).toISOString() : null,
                    to: s.to ? new Date(s.to).toISOString() : null
                }))
            };

            console.log('Form Snapshot:', JSON.parse(JSON.stringify(payload)));

            try {
                const response = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'include',
                    body: JSON.stringify(payload)
                });

                if (response.ok) {
                    this.bsModal.hide();
                    this.showMessage(this.isEditing ? 'Listing updated successfully!' : 'Listing published!', 'success');
                    await this.fetchMySales();
                } else {
                    this.showMessage('Error saving garage sale.', 'danger');
                }
            } catch (err) {
                this.showMessage('Network error while saving.', 'danger');
            } finally {
                this.saving = false;
            }
        }
    };
}